using EZBlocker3.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
            set {
                _process = value;
                if (IsHooked != _wasHooked)
                    OnHookChanged();
                _wasHooked = IsHooked;
            }
        }
        private VolumeControl? _volumeControl;
        public VolumeControl? VolumeControl {
            get {
                _volumeControl ??= AudioUtils.GetVolumeControl(_allSpotifyProcessIds);
                return _volumeControl;
            }
            private set => _volumeControl = value;
        }

        public string? WindowName { get; private set; }

        public bool IsHooked => Process != null && !Process.HasExited;
        public bool IsPaused => State == SpotifyState.Paused;
        public bool IsPlaying => IsSongPlaying || IsAdPlaying;
        public bool IsSongPlaying => State == SpotifyState.PlayingSong;
        public bool IsAdPlaying => State == SpotifyState.PlayingAdvertisement;
        public bool? IsMuted { get; private set; } = null;
        public SongInfo? ActiveSong { get;  private set; }
        public bool IsActive { get; private set; }
        public SpotifyState State { get; private set; } = SpotifyState.Unknown;

        public enum SpotifyState {
            PlayingSong,
            PlayingAdvertisement,
            Paused,
            Unknown
        }

        public event ActiveSongChangedEventHandler? ActiveSongChanged;
        public delegate void ActiveSongChangedEventHandler(object sender, ActiveSongChangedEventArgs eventArgs);
        public event HookChangedEventHandler? HookChanged;
        public delegate void HookChangedEventHandler(object sender, EventArgs eventArgs);
        public event SpotifyStateChangedEventHandler? SpotifyStateChanged;
        public delegate void SpotifyStateChangedEventHandler(object sender, SpotifyStateChangedEventArgs eventArgs);

        private readonly Timer _refreshTimer = new Timer(100);
        private HashSet<int>? _allSpotifyProcessIds;

        public SpotifyHook() {
            _refreshTimer.Elapsed += RefreshTimer_Elapsed;
        }

        public void Activate() {
            if (IsActive)
                return;
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
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e) {
            lock (this) {
                if (!IsHooked) {
                    HookSpotify();
                } else {
                    RefreshHook();
                }
            }
        }

        private bool HookSpotify() {
            Process?.Dispose();

            var processes = Process.GetProcessesByName("spotify");
            // TODO: dispose processes not stored

            Process = processes.Where(e => !string.IsNullOrWhiteSpace(e.MainWindowTitle)).FirstOrDefault();
            if (Process is null) {
                if (VolumeControl is null)
                    return false;

                try {
                    Process = Process.GetProcessById(VolumeControl.ProcessId);
                } catch(ArgumentException) {
                    // TODO rework VolumeControl to store Process so that HasExited can be used.
                    Process = null;
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
            VolumeControl?.Dispose();
            Process?.Dispose();

            Process = null;
            VolumeControl = null;
        }

        private void RefreshHook() {
            if (Process is null)
                return;

            // Process.Refresh();
            var prevProcess = Process;
            Process = Process.GetProcessById(prevProcess.Id);
            prevProcess.Dispose();

            UpdateInfo();
        }

        private void UpdateInfo() {
            UpdateMuteStatus();

            var oldWindowName = WindowName;
            var newWindowName = WindowName = Process?.MainWindowTitle.Trim();
            if (oldWindowName != newWindowName) {
                switch (newWindowName) {
                    case null:
                        // Shuting down
                        // SetShutingDownState();
                        SetUnknownState();
                        break;
                    // Paused / Default for Free version
                    case "Spotify Free":
                    // Paused / Default for Premium version
                    // Why do you need EZBlocker3 when you have premium?
                    case "Spotify Premium":
                        SetPausedState();
                        break;
                    // Advertisment Playing
                    case "Spotify": // Spotify Ads
                    case "Advertisement": // Other Ads
                        // TODO Spotify is also the title on startup
                        SetPlayingAdState();
                        break;
                    // Song Playing: "[artist] - [title]"
                    case var name when name?.Contains('-') == true:
                        var (artist, title) = name.Split('-').Select(e => e.Trim()).ToArray();
                        SetPlayingSongState(new SongInfo(title, artist));
                        break;
                    // What is happening?
                    default:
                        SetUnknownState();
                        break;
                }
            }
        }

        private void SetUnknownState() => UpdateState(SpotifyState.Unknown, newSong: null);
        private void SetPlayingSongState(SongInfo song) => UpdateState(SpotifyState.PlayingSong, newSong: song);
        private void SetPlayingAdState() => UpdateState(SpotifyState.PlayingAdvertisement, newSong: null);
        private void SetPausedState() => UpdateState(SpotifyState.Paused, newSong: null);

        private void UpdateState(SpotifyState newState, SongInfo? newSong) {
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
            } else {
                IsMuted = null;
            }
        }

        private void OnActiveSongChanged(SongInfo? previous, SongInfo? current) =>
            OnActiveSongChanged(new ActiveSongChangedEventArgs(previous, current));
        protected virtual void OnActiveSongChanged(ActiveSongChangedEventArgs eventArgs) {
            ActiveSongChanged?.Invoke(this, eventArgs);
        }

        private void OnHookChanged() =>
           OnHookChanged(EventArgs.Empty);
        protected virtual void OnHookChanged(EventArgs eventArgs) {
            Trace.TraceInformation($"SpotifyHook: Spotify {(IsHooked ? "hooked" : "unhooked")}.");
            HookChanged?.Invoke(this, eventArgs);
        }

        private void OnSpotifyStateChanged(SpotifyState previous, SpotifyState current) =>
            OnSpotifyStateChanged(new SpotifyStateChangedEventArgs(previous, current));
        protected virtual void OnSpotifyStateChanged(SpotifyStateChangedEventArgs eventArgs) {
            SpotifyStateChanged?.Invoke(this, eventArgs);
        }

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
    }

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
}
