using EZBlocker3.Extensions;
using EZBlocker3.Interop;
using EZBlocker3.Logging;
using System;
using System.Diagnostics;
using System.Linq;
using WindowEvent = EZBlocker3.Interop.NativeMethods.WindowEvent;
using AccessibleObjectID = EZBlocker3.Interop.NativeMethods.AccessibleObjectID;
using static EZBlocker3.SpotifyHook;
using EZBlocker3.Audio.ComWrapper;
using EZBlocker3.Audio.Com;

namespace EZBlocker3 {
    public class SpotifyHook : IDisposable {

        public Process? Process { get; private set; }
        public string? WindowTitle { get; private set; }
        internal AudioSession? AudioSession { get; private set; }

        public bool? IsMuted => AudioSession?.IsMuted;

        public bool IsHooked { get; private set; }
        public bool IsPaused => State == SpotifyState.Paused;
        public bool IsPlaying => IsSongPlaying || IsAdPlaying;
        public bool IsSongPlaying => State == SpotifyState.PlayingSong;
        public bool IsAdPlaying => State == SpotifyState.PlayingAdvertisement;
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

        private WindowEventHook _spotifyNameChangeEventHook = new WindowEventHook(WindowEvent.EVENT_OBJECT_NAMECHANGE);
        private WindowEventHook _spotifyObjectDestroyEventHook = new WindowEventHook(WindowEvent.EVENT_OBJECT_DESTROY);
        private WindowEventHook _globalEventHook = new WindowEventHook(WindowEvent.EVENT_OBJECT_CREATE);

        public SpotifyHook() {
            _globalEventHook.WinEventProc += _globalEventHook_WinEventProc;
            _spotifyNameChangeEventHook.WinEventProc += _spotifyNameChangeEventHook_WinEventProc;
            _spotifyObjectDestroyEventHook.WinEventProc += _spotifyObjectDestroyEventHook_WinEventProc;
        }

        public void Activate() {
            if (IsActive)
                throw new InvalidOperationException("Hook is already active.");

            Logger.LogDebug("SpotifyHook: Activated");

            IsActive = true;

            if (!TryHookSpotify())
                _globalEventHook.HookGlobal();
        }

        public void Deactivate() {
            if (!IsActive)
                throw new InvalidOperationException("Hook has to be active.");

            IsActive = false;

            IsHooked = false;
            ClearHookData();

            Logger.LogDebug("SpotifyHook: Deactivated");
        }

        private bool TryHookSpotify() {
            var processes = Process.GetProcessesByName("spotify");
            var mainProcess = processes.Where(process => !string.IsNullOrWhiteSpace(process.MainWindowTitle)).FirstOrDefault();

            if (mainProcess == null)
                return false;

            // dispose unused processes
            foreach (var process in processes)
                if (process != mainProcess)
                    process.Dispose();

            OnSpotifyHooked(mainProcess);

            return true;
        }

        private void _globalEventHook_WinEventProc(IntPtr hWinEventHook, WindowEvent eventType, IntPtr hwnd, AccessibleObjectID idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            if (IsHooked)
                return;
            // make sure that a window was created.
            if (idObject != AccessibleObjectID.OBJID_WINDOW || idChild != NativeMethods.CHILDID_SELF)
                return;

            NativeMethods.GetWindowThreadProcessId(hwnd, out uint processId);
            var process = Process.GetProcessById((int)processId);

            if (!process.ProcessName.Equals("spotify", StringComparison.OrdinalIgnoreCase))
                return;

            OnSpotifyHooked(process);
        }

        private void _spotifyObjectDestroyEventHook_WinEventProc(IntPtr hWinEventHook, WindowEvent eventType, IntPtr hwnd, AccessibleObjectID idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            // make sure that a window was destroyed.
            if (idObject != AccessibleObjectID.OBJID_WINDOW || idChild != NativeMethods.CHILDID_SELF)
                return;
            UpdateState(SpotifyState.ShuttingDown);
            OnSpotifyClose();
        }

        private void _spotifyNameChangeEventHook_WinEventProc(IntPtr hWinEventHook, WindowEvent eventType, IntPtr hwnd, AccessibleObjectID idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            // make sure that a window name has changed.
            if (idObject != AccessibleObjectID.OBJID_WINDOW || idChild != NativeMethods.CHILDID_SELF)
                return;
            UpdateWindowTitle(NativeWindowUtils.GetWindowTitle(hwnd));
        }

        private void OnSpotifyHooked(Process process) {
            if (IsHooked || process == null || process.HasExited)
                return;

            Process = process;
            IsHooked = true;

            if (_globalEventHook.Hooked)
                _globalEventHook.Unhook();

            // TODO multi event hook
            _spotifyNameChangeEventHook.HookToProcess(process);
            _spotifyObjectDestroyEventHook.HookToProcess(process);

            FetchAudioSession();

            OnHookChanged();

            UpdateWindowTitle(Process.MainWindowTitle);
        }

        private void FetchAudioSession() {
            using var device = AudioDevice.GetDefaultAudioDevice(EDataFlow.eRender, ERole.eMultimedia);
            using var sessionManager = device.GetSessionManager();
            using var sessions = sessionManager.GetSessionCollection();
            for (var i=0; i<sessions.Count; i++) {
                AudioSession? session = null;
                try {
                    session = sessions[0];
                    if (session.ProcessID == MainWindowProcess?.Id) {
                        AudioSession = session;
                        break;
                    }
                } catch {
                    session?.Dispose();
                    throw;
                }
            }

            if (AudioSession is null)
                Logger.LogError("SpotifyHook: Failed to fetch audio session.");
        }

        private void UpdateWindowTitle(string newWindowTitle) {
            if (newWindowTitle == WindowTitle)
                return;

            var oldWindowTitle = WindowTitle;
            WindowTitle = newWindowTitle;
            OnWindowTitleChanged(oldWindowTitle, newWindowTitle);
        }

        private void OnWindowTitleChanged(string? oldWindowTitle, string newWindowTitle) {
            Logger.LogDebug($"SpotifyHook: Current window name changed to \"{newWindowTitle}\"");
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
                    UpdateState(SpotifyState.PlayingAdvertisement);
                    break;
                // Advertisment playing or Starting up
                case "Spotify":
                    if (oldWindowTitle is null || oldWindowTitle == "")
                        UpdateState(SpotifyState.StartingUp);
                    else
                        UpdateState(SpotifyState.PlayingAdvertisement);
                    break;
                // Shutting down
                case "":
                    if (oldWindowTitle is null)
                        UpdateState(SpotifyState.StartingUp);
                    else UpdateState(SpotifyState.ShuttingDown);
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

        private void OnSpotifyClose() {
            ClearHookData();

            IsHooked = false;

            OnHookChanged();

            _globalEventHook.HookGlobal();
        }

        protected void ClearHookData() {
            Process?.Dispose();
            AudioSession?.Dispose();

            Process = null;
            WindowTitle = null;
            ActiveSong = null;
            AudioSession = null;
            State = SpotifyState.Unknown;

            if (_globalEventHook.Hooked)
                _globalEventHook.Unhook();
            if (_spotifyNameChangeEventHook.Hooked)
                _spotifyNameChangeEventHook.Unhook();
            if (_spotifyObjectDestroyEventHook.Hooked)
                _spotifyObjectDestroyEventHook.Unhook();
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

        public bool Mute() => SetMute(mute: true);
        public bool Unmute() => SetMute(mute: false);
        public bool SetMute(bool mute) {
            if (!IsHooked)
                return false;

            // ensure audio session
            if (AudioSession is null) {
                FetchAudioSession();
                if (AudioSession is null) {
                    Logger.LogError($"SpotifyHook: Failed to {(mute ? "mute" : "unmute")} spotify due to missing audio session.");
                    return false;
                }
            }

            // mute
            try {
                AudioSession.SetMute(mute);
                Logger.LogInfo($"SpotifyHook: Spotify {(mute ? "muted" : "unmuted")}.");
                return true;
            } catch(Exception e) {
                Logger.LogException($"SpotifyHook: Failed to {(mute ? "mute" : "unmute")} spotify:", e);
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
        public void Dispose() {
            Process?.Dispose();
            _globalEventHook?.Dispose();
            _spotifyNameChangeEventHook?.Dispose();
            _spotifyObjectDestroyEventHook?.Dispose();
        }
        #endregion
    }

    #region EventArgs
    public class SpotifyStateChangedEventArgs : EventArgs {

        public SpotifyState PreviousState { get; private set; }
        public SpotifyState NewState { get; private set; }

        public SpotifyStateChangedEventArgs(SpotifyState previousState, SpotifyState newState) {
            PreviousState = previousState;
            NewState = newState;
        }

    }

    public class ActiveSongChangedEventArgs : EventArgs {

        public SongInfo? PreviousActiveSong { get; private set; }
        public SongInfo? NewActiveSong { get; private set; }

        public ActiveSongChangedEventArgs(SongInfo? previousActiveSong, SongInfo? newActiveSong) {
            PreviousActiveSong = previousActiveSong;
            NewActiveSong = newActiveSong;
        }

    }
    #endregion
}
