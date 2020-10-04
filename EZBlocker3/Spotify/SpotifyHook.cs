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
using EZBlocker3.Utils;
using System.Threading;

namespace EZBlocker3 {
    /// <summary>
    /// Represents a hook to the spotify process.
    /// </summary>
    public class SpotifyHook : IDisposable {

        /// <summary>
        /// The main window process if spotify is running.
        /// </summary>
        public Process? MainWindowProcess { get; private set; }
        /// <summary>
        /// The current window title of the main window.
        /// </summary>
        public string? WindowTitle { get; private set; }
        /// <summary>
        /// The current audio session if spotify is running and a session has been initialized.
        /// </summary>
        internal AudioSession? AudioSession { get; private set; }

        /// <summary>
        /// Gets a value indicating whether spotify is currently muting or null if spotify is not running.
        /// </summary>
        public bool? IsMuted => AudioSession?.IsMuted;

        /// <summary>
        /// Gets a value indicating whether the spotify process is currently hooked.
        /// </summary>
        public bool IsHooked { get; private set; }
        /// <summary>
        /// Gets a value indicating whether spotify is currently paused.
        /// </summary>
        public bool IsPaused => State == SpotifyState.Paused;
        /// <summary>
        /// Gets a value indicating whether spotify is currently playing.
        /// </summary>
        public bool IsPlaying => IsSongPlaying || IsAdPlaying;
        /// <summary>
        /// Gets a value indicating whether spotify is currently playing a song.
        /// </summary>
        public bool IsSongPlaying => State == SpotifyState.PlayingSong;
        /// <summary>
        /// Gets a value indicating whether spotify is currently playing an advertisement.
        /// </summary>
        public bool IsAdPlaying => State == SpotifyState.PlayingAdvertisement;
        /// <summary>
        /// Gets the currently playing song or null if no song is being played or spotify is not running.
        /// </summary>
        public SongInfo? ActiveSong { get; private set; }
        /// <summary>
        /// Gets a value indicating whether this object is active and looking for a spotify process to attach to.
        /// </summary>
        public bool IsActive { get; private set; }
        /// <summary>
        /// Gets a value indicating the current state of a running spotify process.
        /// </summary>
        public SpotifyState State { get; private set; } = SpotifyState.Unknown;

        /// <summary>
        /// Represents the current state of a running spotify process.
        /// </summary>
        public enum SpotifyState {
            /// <summary>
            /// Spotify is in an unknown state.
            /// </summary>
            Unknown,
            /// <summary>
            /// Spotify is playing a song.
            /// </summary>
            PlayingSong,
            /// <summary>
            /// Spotify is playing an advertisement.
            /// </summary>
            PlayingAdvertisement,
            /// <summary>
            /// Spotify is paused.
            /// </summary>
            Paused,
            /// <summary>
            /// Spotify is in the process of starting up.
            /// </summary>
            StartingUp,
            /// <summary>
            /// Spotify is in the process of shutting down.
            /// </summary>
            ShuttingDown
        }

        /// <summary>
        /// Occurs whenever the currently playing song changes.
        /// </summary>
        public event ActiveSongChangedEventHandler? ActiveSongChanged;
        /// <summary>
        /// Represents an event handler for the ActiveSongChanged event.
        /// </summary>
        public delegate void ActiveSongChangedEventHandler(object sender, ActiveSongChangedEventArgs eventArgs);
        /// <summary>
        /// Occurs whenever a new spotify process is hooked or an exisiting one is unhooked.
        /// </summary>
        public event HookChangedEventHandler? HookChanged;
        /// <summary>
        /// Represents an event handler for the HookChanged event.
        /// </summary>
        public delegate void HookChangedEventHandler(object sender, EventArgs eventArgs);
        /// <summary>
        /// Occurs whenever spotify changes its state.
        /// </summary>
        public event SpotifyStateChangedEventHandler? SpotifyStateChanged;
        /// <summary>
        /// Represents an event handler for the SpotifyStateChanged event.
        /// </summary>
        public delegate void SpotifyStateChangedEventHandler(object sender, SpotifyStateChangedEventArgs eventArgs);

        private readonly WindowEventHook _spotifyNameChangeEventHook = new WindowEventHook(WindowEvent.EVENT_OBJECT_NAMECHANGE);
        private readonly WindowEventHook _spotifyObjectDestroyEventHook = new WindowEventHook(WindowEvent.EVENT_OBJECT_DESTROY);
        private readonly WindowEventHook _globalEventHook = new WindowEventHook(WindowEvent.EVENT_OBJECT_CREATE);

        /// <summary>
        /// A cache for all running spotify processes.
        /// </summary>
        private Process[]? _processesCache;

        /// <summary>
        /// Creates a new inactive spotify hook.
        /// </summary>
        public SpotifyHook() {
            _globalEventHook.WinEventProc += _globalEventHook_WinEventProc;
            _spotifyNameChangeEventHook.WinEventProc += _spotifyNameChangeEventHook_WinEventProc;
            _spotifyObjectDestroyEventHook.WinEventProc += _spotifyObjectDestroyEventHook_WinEventProc;
        }

        /// <summary>
        /// Activates the hook and starts looking for a running spotify process.
        /// </summary>
        /// <exception cref="InvalidOperationException">The hook is already active.</exception>
        public void Activate() {
            if (IsActive)
                throw new InvalidOperationException("Hook is already active.");

            Logger.LogDebug("SpotifyHook: Activated");

            IsActive = true;

            _globalEventHook.HookGlobal();
            TryHookSpotify();
        }

        /// <summary>
        /// Deactivates the hook, clears hook data and stops looking for a running spotify process.
        /// </summary>
        /// <exception cref="InvalidOperationException">The hook is not active.</exception>
        public void Deactivate() {
            if (!IsActive)
                throw new InvalidOperationException("Hook has to be active.");

            IsActive = false;

            IsHooked = false;
            ClearHookData();

            Logger.LogDebug("SpotifyHook: Deactivated");
        }

        /// <summary>
        /// Try hooking to a currently running spotify process.
        /// </summary>
        /// <returns>A value indicating whether spotify could be hooked.</returns>
        protected bool TryHookSpotify() {
            var processes = Process.GetProcessesByName("spotify");

            // find the main window process
            var mainProcess = processes.Where(process => !string.IsNullOrWhiteSpace(process.MainWindowTitle)).FirstOrDefault();

            if (mainProcess == null)
                return false;

            // cache the process list for fetching the audio session
            _processesCache = processes;

            OnSpotifyHooked(mainProcess);

            return true;
        }

        private readonly object _lock_globalEventHook = new object();
        private void _globalEventHook_WinEventProc(IntPtr hWinEventHook, WindowEvent eventType, IntPtr hwnd, AccessibleObjectID idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            if (IsHooked)
                return;
            // make sure that the created control is a window.
            if (idObject != AccessibleObjectID.OBJID_WINDOW || idChild != NativeMethods.CHILDID_SELF)
                return;

            lock (_lock_globalEventHook) {
                // recheck state in case it has changed in another thread.
                if (IsHooked)
                    return;

                NativeMethods.GetWindowThreadProcessId(hwnd, out uint processId);
                var process = Process.GetProcessById((int)processId);

                if (!process.ProcessName.Equals("spotify", StringComparison.OrdinalIgnoreCase))
                    return;

                OnSpotifyHooked(process);
                UpdateState(SpotifyState.StartingUp);
            }
        }

        private readonly object _lock_spotifyObjectDestroyEventHook = new object();
        private void _spotifyObjectDestroyEventHook_WinEventProc(IntPtr hWinEventHook, WindowEvent eventType, IntPtr hwnd, AccessibleObjectID idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            // make sure that the destroyed control was a window.
            if (idObject != AccessibleObjectID.OBJID_WINDOW || idChild != NativeMethods.CHILDID_SELF)
                return;

            lock (_lock_spotifyObjectDestroyEventHook) {
                // recheck state in case it has changed since the event was raised.
                if (!IsHooked)
                    return;

                UpdateState(SpotifyState.ShuttingDown);
                OnSpotifyClosed();
            }
        }

        private readonly object _lock_spotifyNameChangeEventHook = new object();
        private void _spotifyNameChangeEventHook_WinEventProc(IntPtr hWinEventHook, WindowEvent eventType, IntPtr hwnd, AccessibleObjectID idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            // make sure that it was a window name that changed.
            if (idObject != AccessibleObjectID.OBJID_WINDOW || idChild != NativeMethods.CHILDID_SELF)
                return;

            lock (_lock_spotifyNameChangeEventHook) {
                // recheck state in case it has changed since the event was raised.
                if (!IsHooked)
                    return;

                UpdateWindowTitle(NativeWindowUtils.GetWindowTitle(hwnd));
            }
        }

        /// <summary>
        /// OnSpotifyHooked is called whenever spotify is hooked.
        /// </summary>
        /// <param name="mainProcess">The main window spotify process.</param>
        private readonly object _lock_OnSpotifyHooked = new object();
        protected virtual void OnSpotifyHooked(Process mainProcess) {
            if (mainProcess == null || mainProcess.HasExited)
                return;

            // avoid "double-hooking"
            // this lock could be replaced with Interlocked.Exchange but it does not support bools.
            lock (_lock_OnSpotifyHooked) {
                if (IsHooked)
                    return;
                IsHooked = true;
            }

            MainWindowProcess = mainProcess;

            if (_globalEventHook.Hooked)
                _globalEventHook.Unhook();

            // TODO multi event hook
            _spotifyNameChangeEventHook.HookToProcess(mainProcess);
            _spotifyObjectDestroyEventHook.HookToProcess(mainProcess);

            // FetchAudioSession();

            OnHookChanged();

            UpdateWindowTitle(mainProcess.MainWindowTitle);
        }

        /// <summary>
        /// Fetch the spotify audio session.
        /// </summary>
        protected void FetchAudioSession() {
            // Fetch sessions
            using var device = AudioDevice.GetDefaultAudioDevice(EDataFlow.eRender, ERole.eMultimedia);
            using var sessionManager = device.GetSessionManager();
            using var sessions = sessionManager.GetSessionCollection();

            // Check main process
            var sessionCount = sessions.Count;
            using var sessionCache = new DisposableList<AudioSession>(sessionCount);
            for (var i=0; i<sessions.Count; i++) {
                var session = sessions[i];
                if (session.ProcessID == MainWindowProcess?.Id) {
                    Logger.LogInfo("SpotifyHook: Successfully fetched audio session using main window process.");
                    AudioSession = session;
                    return;
                } else {
                    // Store non-spotify sessions in disposable list to make sure that they the underlying COM objects are disposed.
                    sessionCache.Add(session);
                }
            }

            Logger.LogWarning("SpotifyHook: Failed to fetch audio session using main window process.");

            // Try fetch through other "spotify" processes.
            var processes = FetchSpotifyProcesses();

            // Transfer the found sessions into a dictionary to speed up the search by process id.
            // (we do this here to avoid the overhead as most of the time we will find the session in the code above.)
            using var sessionMap = new ValueDisposableDictionary<uint, AudioSession>();
            foreach (var session in sessionCache)
                sessionMap.Add(session.ProcessID, session);
            sessionCache.Clear();

            foreach (var process in processes) {
                var processId = (uint)process.Id;

                // skip main process as we already checked it
                if (MainWindowProcess?.Id == processId)
                    continue;

                if (sessionMap.TryGetValue(processId, out AudioSession session)) {
                    AudioSession = session;
                    Logger.LogInfo("SpotifyHook: Successfully fetched audio session using secondary spotify processes.");

                    // remove from map to avoid disposal
                    sessionMap.Remove(processId);
                    return;
                }
            }

            Logger.LogError("SpotifyHook: Failed to fetch audio session.");
        }

        /// <summary>
        /// Fetches all running processes with the name "spotify". This method uses caching, but makes sure that the returned processes are running.
        /// </summary>
        /// <returns>An array of running spotify processes.</returns>
        protected Process[] FetchSpotifyProcesses() {
            if (_processesCache is null) {
                _processesCache = Process.GetProcessesByName("spotify");
                return _processesCache; 
            }

            if (_processesCache.Any(process => !process.IsAssociated() || process.HasExited))
                _processesCache = Process.GetProcessesByName("spotify");

            return _processesCache;
        }

        /// <summary>
        /// Update the current window title to a new value.
        /// </summary>
        /// <param name="newWindowTitle">The new spotify window title.</param>
        protected void UpdateWindowTitle(string newWindowTitle) {
            if (newWindowTitle == WindowTitle)
                return;

            var oldWindowTitle = WindowTitle;
            WindowTitle = newWindowTitle;
            OnWindowTitleChanged(oldWindowTitle, newWindowTitle);
        }

        /// <summary>
        /// OnWindowTitleChanged is called whenever the main spotify window changes its title.
        /// </summary>
        /// <param name="oldWindowTitle">The old window title.</param>
        /// <param name="newWindowTitle">The new window title.</param>
        protected virtual void OnWindowTitleChanged(string? oldWindowTitle, string newWindowTitle) {
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
                    else
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

        /// <summary>
        /// OnSpotifyClosed is called whenever spotify is hooked.
        /// </summary>
        protected virtual void OnSpotifyClosed() {
            Logger.LogWarning($"SpotifyHook: Spotify closed.");

            ClearHookData();

            IsHooked = false;

            OnHookChanged();

            _globalEventHook.HookGlobal();
        }

        /// <summary>
        /// Clears all the state associated with a hooked spotify process.
        /// </summary>
        protected void ClearHookData() {
            MainWindowProcess?.Dispose();
            _processesCache?.DisposeAll();
            AudioSession?.Dispose();

            MainWindowProcess = null;
            _processesCache = null;
            AudioSession = null;
            WindowTitle = null;
            ActiveSong = null;
            State = SpotifyState.Unknown;

            if (_globalEventHook.Hooked)
                _globalEventHook.Unhook();
            if (_spotifyNameChangeEventHook.Hooked)
                _spotifyNameChangeEventHook.Unhook();
            if (_spotifyObjectDestroyEventHook.Hooked)
                _spotifyObjectDestroyEventHook.Unhook();
        }

        /// <summary>
        /// Updates the current state of the spotify process.
        /// </summary>
        /// <param name="newState">The new state of the spotify process.</param>
        /// <param name="newSong">The currently playing song, if applicable.</param>
        protected void UpdateState(SpotifyState newState, SongInfo? newSong = null) {
            var prevSong = ActiveSong;
            var prevState = State;
            ActiveSong = newSong;
            State = newState;

            if (prevState != newState)
                OnSpotifyStateChanged(prevState, newState);
            if (prevSong != newSong)
                OnActiveSongChanged(prevSong, newSong);
        }

        /// <summary>
        /// Mutes the spotify audio session.
        /// </summary>
        /// <returns>A value indicating whether the operation was successful</returns>
        public bool Mute() => SetMute(mute: true);
        /// <summary>
        /// Unmutes the spotify audio session.
        /// </summary>
        /// <returns>A value indicating whether the operation was successful</returns>
        public bool Unmute() => SetMute(mute: false);
        /// <summary>
        /// Sets the spotify mute status to the given state.
        /// </summary>
        /// <param name="mute">A value indicating whether spotify should be muted or unmuted.</param>
        /// <returns>A value indicating whether the operation was successful</returns>
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
        /// <summary>
        /// OnActiveSongChanged is called whenever the currently active song changes.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected virtual void OnActiveSongChanged(ActiveSongChangedEventArgs eventArgs) {
            Logger.LogInfo($"SpotifyHook: Active song: \"{eventArgs.NewActiveSong}\"");
            ActiveSongChanged?.Invoke(this, eventArgs);
        }

        private void OnHookChanged() =>
           OnHookChanged(EventArgs.Empty);
        /// <summary>
        /// OnHookChanged is called whenever spotify is hooked or unhooked.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected virtual void OnHookChanged(EventArgs eventArgs) {
            Logger.LogInfo($"SpotifyHook: Spotify {(IsHooked ? "hooked" : "unhooked")}.");
            HookChanged?.Invoke(this, eventArgs);
        }

        private void OnSpotifyStateChanged(SpotifyState previous, SpotifyState current) =>
            OnSpotifyStateChanged(new SpotifyStateChangedEventArgs(previous, current));
        /// <summary>
        /// OnSpotifyStateChanged is called whenever the current state of spotify changes.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected virtual void OnSpotifyStateChanged(SpotifyStateChangedEventArgs eventArgs) {
            Logger.LogInfo($"SpotifyHook: Spotify is in {eventArgs.NewState} state.");
            SpotifyStateChanged?.Invoke(this, eventArgs);
        }

        public void Dispose() {
            MainWindowProcess?.Dispose();
            _processesCache?.DisposeAll();
            AudioSession?.Dispose();
            _globalEventHook?.Dispose();
            _spotifyNameChangeEventHook?.Dispose();
            _spotifyObjectDestroyEventHook?.Dispose();
        }
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
