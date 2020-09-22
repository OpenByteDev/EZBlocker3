using EZBlocker3.Extensions;
using EZBlocker3.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Timers;
using static EZBlocker3.AudioUtils;
using static EZBlocker3.SpotifyHook;
using Timer = System.Timers.Timer;

namespace EZBlocker3 {
    internal class SpotifyHook : IDisposable {

        private Process? _process;
        private bool _wasHooked = false;
        public Process? Process {
            get => _process;
            private set {
                _process = value;
                if (IsHooked != _wasHooked)
                    OnHookChanged();

                _wasHooked = IsHooked;
            }
        }
        private VolumeControl? _volumeControl;
        private HashSet<int>? _allSpotifyProcessIds;
        public VolumeControl? VolumeControl {
            get {
                _volumeControl ??= AudioUtils.GetVolumeControl(_allSpotifyProcessIds);
                return _volumeControl;
            }
            private set => _volumeControl = value;
        }

        public string? WindowTitle { get; private set; } = string.Empty;

        public bool IsHooked {
            get {
                try {
                    return Process != null && !Process.HasExited;
                } catch (InvalidOperationException) { // throws on unassociated process.
                    Process = null; // avoid the exception next time
                    return false;
                }
            }
        }
        public bool IsPaused => State == SpotifyState.Paused;
        public bool IsPlaying => IsSongPlaying || IsAdPlaying;
        public bool IsSongPlaying => State == SpotifyState.PlayingSong;
        public bool IsAdPlaying => State == SpotifyState.PlayingAdvertisement;
        public bool? IsMuted { get; private set; } = null;
        public SongInfo? ActiveSong { get; private set; }
        public bool IsActive { get; private set; }
        public SpotifyState State { get; private set; } = SpotifyState.Unknown;

        public enum SpotifyState {
            PlayingSong,
            PlayingAdvertisement,
            Paused,
            StartingUp,
            ShuttingDown,
            Unknown
        }

        public event ActiveSongChangedEventHandler? ActiveSongChanged;
        public delegate void ActiveSongChangedEventHandler(object sender, ActiveSongChangedEventArgs eventArgs);
        public event HookChangedEventHandler? HookChanged;
        public delegate void HookChangedEventHandler(object sender, EventArgs eventArgs);
        public event SpotifyStateChangedEventHandler? SpotifyStateChanged;
        public delegate void SpotifyStateChangedEventHandler(object sender, SpotifyStateChangedEventArgs eventArgs);

        public const double HookedRefreshInterval = 500;
        public const double UnhookedRefreshInterval = 2000;
        private readonly Timer _refreshTimer = new Timer(UnhookedRefreshInterval);

        public SpotifyHook() {
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
            _refreshTimer.AutoReset = true;
        }

        public void Activate() {
            if (IsActive)
                return;

            Logger.LogDebug("SpotifyHook: Activated");

            IsActive = true;

            HookSpotify();

            _refreshTimer.Start();

        }
        public void Deactivate() {
            if (!IsActive)
                return;

            IsActive = false;

            _refreshTimer.Stop();

            ClearHook();

            Logger.LogInfo($"SpotifyHook: Deactivated");
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e) {
            if (!IsHooked) {
                HookSpotify();
            } else {
                RefreshHook();
            }
            _refreshTimer.Interval = IsHooked ? HookedRefreshInterval : UnhookedRefreshInterval;

            _refreshTimer.Enabled = true;
        }

        private bool HookSpotify() {
            ClearHook();

            var processes = Process.GetProcessesByName("spotify");
            // TODO: dispose processes not stored

            Process = processes.Where(e => !string.IsNullOrWhiteSpace(e.MainWindowTitle)).FirstOrDefault();
            if (Process is null) {
                if (VolumeControl is null)
                    return false;

                try {
                    Process = Process.GetProcessById(VolumeControl.ProcessId);
                } catch (ArgumentException) {
                    // TODO rework VolumeControl to store Process so that HasExited can be used.
                    Process = null;

                    Logger.LogInfo($"SpotifyHook: Failed to recover hook using volume control.");
                }

                if (!IsHooked) {
                    Process = null;
                    VolumeControl = null;
                    return false;
                }
            }

            _allSpotifyProcessIds = processes.Select(e => e.Id).ToHashSet();
            UpdateInfo();

            return true;
        }

        private void ClearHook() {
            Process?.Dispose();
            VolumeControl?.Dispose();

            Process = null;
            VolumeControl = null;
            IsMuted = null;

            // Logger.LogDebug($"SpotifyHook: Cleared");
        }

        private Action<Process>? _invalidateProcessMainWindowName;
        private void RefreshHook() {
            if (Process is null)
                return;
            // Logger.LogDebug($"Start refreshing Spotify Hook");

            if (_invalidateProcessMainWindowName is null) {
                // Expression Trees let us change a private field and are faster than reflection (if called multiple times)
                var processParamter = Expression.Parameter(typeof(Process), "process");
                var mainWindowField = Expression.Field(processParamter, "mainWindowTitle");
                var assignment = Expression.Assign(mainWindowField, Expression.Constant(null, typeof(string)));
                var lambda = Expression.Lambda<Action<Process>>(assignment, processParamter);
                _invalidateProcessMainWindowName = lambda.Compile();
            }
            try {
                // invalidate MainWindowTitle by setting the backing field to null (does not happen by calling .Refresh())
                _invalidateProcessMainWindowName(Process);

                // invalidate other cached information
                Process.Refresh();
            } catch (Exception e) {
                Logger.LogError("SpotifyHook: Exception during fast invalidation of MainWindowTitle:\n" + e);

                // custom invalidation failed -> fall back to slower workaround.
                var prevProcess = Process;
                Process = Process.GetProcessById(prevProcess.Id);
                prevProcess.Dispose();
            }

            // inspect updated process information
            UpdateInfo();

            // Logger.LogDebug($"Refreshed Spotify Hook");
        }

        private void UpdateInfo() {
            UpdateMuteStatus();

            var oldWindowTitle = WindowTitle;
            var newWindowTitle = WindowTitle = Process?.MainWindowTitle.Trim();
            if (oldWindowTitle != newWindowTitle) {
                Logger.LogDebug($"SpotifyHook: Current window name is \"{newWindowTitle}\"");
                switch (newWindowTitle) {
                    // Paused / Default for Free version
                    case "Spotify Free":
                    // Paused / Default for Premium version
                    // Why do you need EZBlocker3 when you have premium?
                    case "Spotify Premium":
                        UpdateState(SpotifyState.Paused);
                        break;
                    // Advertisment Playing
                    case "Advertisement":
                    case "Spotify" when oldWindowTitle != "":
                        UpdateState(SpotifyState.PlayingAdvertisement);
                        break;
                    // Starting up
                    case "Spotify" when oldWindowTitle == "":
                        UpdateState(SpotifyState.StartingUp);
                        break;
                    // Shutting down
                    case "":
                        UpdateState(SpotifyState.ShuttingDown);
                        break;
                    // Song Playing: "[artist] - [title]"
                    case var name when name?.Contains(" - ") == true:
                        var (artist, title) = name.Split(" - ", maxCount: 2).Select(e => e.Trim()).ToArray();
                        UpdateState(SpotifyState.PlayingSong, newSong: new SongInfo(title, artist));
                        break;
                    // What is happening?
                    default:
                        UpdateState(SpotifyState.Unknown);
                        Logger.LogWarning($"SpotifyHook: Spotify entered an unknown state. (WindowTitle={newWindowTitle})");
                        break;
                }
            }
        }

        private void UpdateState(SpotifyState newState, SongInfo? newSong = null) {
            var prevSong = ActiveSong;
            var prevState = State;
            ActiveSong = newSong;
            State = newState;

            if (prevState != newState)
                OnSpotifyStateChanged(prevState, newState);
            if (prevSong != newSong)
                OnActiveSongChanged(prevSong, newSong);
        }

        private void UpdateMuteStatus() {
            IsMuted ??= AudioUtils.IsMuted(VolumeControl?.Control);
        }

        public bool Mute() => SetMute(mute: true);
        public bool Unmute() => SetMute(mute: false);
        public bool SetMute(bool mute) {
            if (!IsHooked)
                return false;

            if (IsMuted == mute)
                return true;

            if (VolumeControl != null) {
                AudioUtils.SetMute(VolumeControl.Control, mute);
                IsMuted = mute;
                Logger.LogInfo($"SpotifyHook: Spotify {(mute ? "muted" : "unmuted")}.");
                return true;
            } else {
                IsMuted = null;
                Logger.LogWarning($"SpotifyHook: Failed to {(mute ? "mute" : "unmute")} Spotify due to missing volume control.");
                return false;
            }
        }

        private void OnActiveSongChanged(SongInfo? previous, SongInfo? current) =>
            OnActiveSongChanged(new ActiveSongChangedEventArgs(previous, current));
        protected virtual void OnActiveSongChanged(ActiveSongChangedEventArgs eventArgs) {
            Logger.LogInfo($"SpotifyHook: Active song: \"{eventArgs.NewActiveSong}\"");
            ActiveSongChanged?.Invoke(this, eventArgs);
        }

        private void OnHookChanged() =>
           OnHookChanged(EventArgs.Empty);
        protected virtual void OnHookChanged(EventArgs eventArgs) {
            Logger.LogInfo($"SpotifyHook: Spotify {(IsHooked ? "hooked" : "unhooked")}.");
            HookChanged?.Invoke(this, eventArgs);
        }

        private void OnSpotifyStateChanged(SpotifyState previous, SpotifyState current) =>
            OnSpotifyStateChanged(new SpotifyStateChangedEventArgs(previous, current));
        protected virtual void OnSpotifyStateChanged(SpotifyStateChangedEventArgs eventArgs) {
            Logger.LogInfo($"SpotifyHook: Spotify is in {eventArgs.NewState} state.");
            SpotifyStateChanged?.Invoke(this, eventArgs);
        }

        #region IDisposable
        private bool _disposed;

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // dispose managed state
                    _refreshTimer?.Dispose();
                    VolumeControl?.Dispose();
                    Process?.Dispose();
                }

                // free unmanaged resources

                _disposed = true;
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    #region EventArgs
    internal class SpotifyStateChangedEventArgs : EventArgs {

        public SpotifyState PreviousState { get; private set; }
        public SpotifyState NewState { get; private set; }

        public SpotifyStateChangedEventArgs(SpotifyState previousState, SpotifyState newState) {
            PreviousState = previousState;
            NewState = newState;
        }

    }

    internal class ActiveSongChangedEventArgs : EventArgs {

        public SongInfo? PreviousActiveSong { get; private set; }
        public SongInfo? NewActiveSong { get; private set; }

        public ActiveSongChangedEventArgs(SongInfo? previousActiveSong, SongInfo? newActiveSong) {
            PreviousActiveSong = previousActiveSong;
            NewActiveSong = newActiveSong;
        }

    }
    #endregion
}
