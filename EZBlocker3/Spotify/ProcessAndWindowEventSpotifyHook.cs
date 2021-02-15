using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EZBlocker3.Audio.CoreAudio;
using EZBlocker3.Extensions;
using EZBlocker3.Interop;
using EZBlocker3.Logging;
using EZBlocker3.Utils;
using Lazy;
using Microsoft.Windows.Sdk;
using WinEventHook;

namespace EZBlocker3.Spotify {
    /// <summary>
    /// Represents a hook to the spotify process.
    /// </summary>
    public class ProcessAndWindowEventSpotifyHook : AbstractSpotifyHook, IMutingSpotifyHook {
        /// <summary>
        /// The main window process if spotify is running.
        /// </summary>
        public Process? MainWindowProcess { get; private set; }
        /// <summary>
        /// The current window title of the main window.
        /// </summary>
        public string? WindowTitle { get; private set; }

        /// <summary>
        /// Gets a value indicating whether spotify is currently muting or null if spotify is not running.
        /// </summary>
        public bool? IsMuted => AudioSession?.IsMuted;

        /// <summary>
        /// The current audio session if spotify is running and a session has been initialized.
        /// </summary>
        private AudioSession? _audioSession;
        public AudioSession? AudioSession => _audioSession ??= FetchAudioSession();

        public bool AssumeAdOnUnknownState { get; init; }

        private readonly WindowEventHook _titleChangeEventHook = new(WindowEvent.EVENT_OBJECT_NAMECHANGE);
        private readonly WindowEventHook _windowDestructionEventHook = new(WindowEvent.EVENT_OBJECT_DESTROY);
        private readonly WindowEventHook _windowCreationEventHook = new(WindowEvent.EVENT_OBJECT_SHOW);

        private readonly ReentrancySafeEventProcessor<IntPtr> _windowCreationEventProcessor;
        private readonly ReentrancySafeEventProcessor<IntPtr> _titleChangeEventProcessor;
        private readonly ReentrancySafeEventProcessor<IntPtr> _windowDestructionEventProcessor;

        /// <summary>
        /// Creates a new inactive spotify hook.
        /// </summary>
        public ProcessAndWindowEventSpotifyHook() {
            _windowCreationEventHook.EventReceived += WindowCreationEventReceived;
            _titleChangeEventHook.EventReceived += TitleChangeEventReceived;
            _windowDestructionEventHook.EventReceived += WindowDestructionEventReceived;

            _windowCreationEventProcessor = new ReentrancySafeEventProcessor<IntPtr>(HandleWindowCreation);
            _titleChangeEventProcessor = new ReentrancySafeEventProcessor<IntPtr>(HandleWindowTitleChange);
            _windowDestructionEventProcessor = new ReentrancySafeEventProcessor<IntPtr>(HandleWindowDestruction);
        }

        /// <summary>
        /// Activates the hook and starts looking for a running spotify process.
        /// </summary>
        /// <exception cref="InvalidOperationException">The hook is already active.</exception>
        public override void Activate() {
            if (IsActive)
                throw new InvalidOperationException("Hook is already active.");

            Logger.Hook.LogDebug("Activated");

            IsActive = true;

            _windowCreationEventHook.HookGlobal();
            TryHookSpotify();
        }

        /// <summary>
        /// Deactivates the hook, clears hook data and stops looking for a running spotify process.
        /// </summary>
        /// <exception cref="InvalidOperationException">The hook is not active.</exception>
        public override void Deactivate() {
            if (!IsActive)
                throw new InvalidOperationException("Hook has to be active.");

            IsActive = false;

            IsHooked = false;
            ClearHookData();

            Logger.Hook.LogDebug("Deactivated");
        }

        /// <summary>
        /// Try hooking to a currently running spotify process.
        /// </summary>
        /// <returns>A value indicating whether spotify could be hooked.</returns>
        protected bool TryHookSpotify() {
            var processes = FetchSpotifyProcesses();

            // find the main window process
            var mainProcess = Array.Find(processes, p => SpotifyProcessUtils.IsMainWindowSpotifyProcess(p));

            if (mainProcess == null)
                return false;

            if (IsHooked)
                return true;

            OnSpotifyHooked(mainProcess);

            return true;
        }

        private static bool IsWindowEvent(WinEventHookEventArgs eventArgs) {
            return eventArgs.ObjectId == AccessibleObjectID.OBJID_WINDOW && eventArgs.IsOwnEvent;
        }

        private void WindowCreationEventReceived(object sender, WinEventHookEventArgs e) {
            // ignore event if we are already hooked.
            if (IsHooked)
                return;

            // make sure that the created control is a window.
            if (!IsWindowEvent(e))
                return;

            // queue events and handle one after another
            // needed because this method gets called multiple times by the same thread at the same time (reentrant)
            _windowCreationEventProcessor.EnqueueAndProcess(e.WindowHandle);
        }

        [Lazy]
        private static Func<uint, Process> _getProcessByIdFastFunc {
            get {
                // Expression Trees let us change a private field and are faster than reflection (if called multiple times)
                var processIdParameter = Expression.Parameter(typeof(uint), "processId");
                var processInfoType = typeof(Process).Assembly.GetType(typeof(Process).FullName + "Info");
                var constructor = typeof(Process).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, new Type[] { typeof(string), typeof(bool), typeof(int), processInfoType });
                var processIdConverted = Expression.Convert(processIdParameter, typeof(int));
                var newExpression = Expression.New(constructor, Expression.Constant("."), Expression.Constant(false), processIdConverted, Expression.Constant(null, processInfoType));
                var lambda = Expression.Lambda<Func<uint, Process>>(newExpression, processIdParameter);
                return lambda.Compile();
            }
        }

        private void HandleWindowCreation(IntPtr windowHandle) {
            // get created process
            var processId = NativeUtils.GetWindowThreadProcessId(windowHandle);

            // avoid semi costly validation checks
            var process = _getProcessByIdFastFunc(processId);

            // confirm that its a spotify process with a window.
            if (!SpotifyProcessUtils.IsMainWindowSpotifyProcess(process)) {
                process.Dispose();
                return;
            }

            // confirm the process still runs
            // if (process.HasExited)
            //    return;

            // confirm that we have the correct window.
            // if (GetWindowClassName(windowHandle) != "OleMainThreadWndClass")
            //    return;

            // ignore the "Start with Spotify" proxy.
            // if (process.MainModule.FileVersionInfo.InternalName != "Spotify")
            //    return;

            OnSpotifyHooked(process);

            // ignore later events
            _windowCreationEventProcessor.FlushQueue();
        }

        private void WindowDestructionEventReceived(object sender, WinEventHookEventArgs e) {
            // ignore event if we are already unhooked.
            if (!IsHooked)
                return;

            // make sure that the destroyed control was a window.
            if (!IsWindowEvent(e))
                return;

            // queue events and handle one after another
            // needed because this method gets called multiple times by the same thread at the same time (reentrant)
            _windowDestructionEventProcessor.EnqueueAndProcess(e.WindowHandle);
        }

        private void HandleWindowDestruction(IntPtr windowHandle) {
            _windowDestructionEventProcessor.FlushQueue();

            if (MainWindowProcess == null)
                return;

            if (!MainWindowProcess.HasExited)
                return;

            UpdateSpotifyState(SpotifyState.ShuttingDown);
            OnSpotifyClosed();
        }

        private void TitleChangeEventReceived(object sender, WinEventHookEventArgs e) {
            // ignore event if we are not hooked.
            if (!IsHooked)
                return;

            // make sure that it was a window name that changed.
            if (!IsWindowEvent(e))
                return;

            // queue events and handle one after another
            // needed because this method gets called multiple times by the same thread at the same time (reentrant)
            _titleChangeEventProcessor.EnqueueAndProcess(e.WindowHandle);
        }

        private void HandleWindowTitleChange(IntPtr windowHandle) {
            UpdateWindowTitle(NativeUtils.GetWindowTitle(windowHandle));
        }

        /// <summary>
        /// OnSpotifyHooked is called whenever spotify is hooked.
        /// </summary>
        /// <param name="mainProcess">The main window spotify process.</param>
        protected virtual void OnSpotifyHooked(Process mainProcess) {
            // ignore if already hooked
            if (IsHooked)
                return;

            MainWindowProcess = mainProcess;

            if (_windowCreationEventHook.Hooked)
                _windowCreationEventHook.Unhook();

            _titleChangeEventHook.HookToProcess(mainProcess);
            _windowDestructionEventHook.HookToProcess(mainProcess);

            mainProcess.EnableRaisingEvents = true;
            mainProcess.Exited += (s, e) => OnSpotifyClosed();

            IsHooked = true;

            UpdateWindowTitle(NativeUtils.GetMainWindowTitle(mainProcess)!);
        }

        /// <summary>
        /// Fetch the spotify audio session.
        /// </summary>
        internal AudioSession? FetchAudioSession() {
            // Fetch sessions
            using var device = AudioDevice.GetDefaultAudioDevice(EDataFlow.eRender, ERole.eMultimedia);
            using var sessionManager = device.GetSessionManager();
            using var sessions = sessionManager.GetSessionCollection();

            // Check main process
            var sessionCount = sessions.Count;
            using var sessionCache = new DisposableList<AudioSession>(sessionCount);
            for (var i = 0; i < sessions.Count; i++) {
                var session = sessions[i];
                if (session.ProcessID == MainWindowProcess?.Id) {
                    Logger.Hook.LogInfo("Successfully fetched audio session using main window process.");
                    return session;
                } else {
                    // Store non-spotify sessions in disposable list to make sure that they the underlying COM objects are disposed.
                    sessionCache.Add(session);
                }
            }

            Logger.Hook.LogWarning("Failed to fetch audio session using main window process.");

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
                    Logger.Hook.LogInfo("Successfully fetched audio session using secondary spotify processes.");

                    // remove from map to avoid disposal
                    sessionMap.Remove(processId);
                    return _audioSession;
                }
            }

            Logger.Hook.LogError("Failed to fetch audio session.");

            return null;
        }

        /// <summary>
        /// A cache for all running spotify processes.
        /// </summary>
        private Process[]? _processesCache;

        /// <summary>
        /// Fetches all running processes with the name "spotify". This method uses caching, but makes sure that the returned processes are running.
        /// </summary>
        /// <returns>An array of running spotify processes.</returns>
        internal Process[] FetchSpotifyProcesses() {
            if (_processesCache is null || _processesCache.Any(process => !process.IsAssociated() || process.HasExited))
                return SpotifyProcessUtils.GetSpotifyProcesses().ToArray();

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
            Logger.Hook.LogDebug($"Current window name changed to \"{newWindowTitle}\"");
            switch (newWindowTitle) {
                // Paused / Default for Free version
                case "Spotify Free":
                // Paused / Default for Premium version
                // Why do you need EZBlocker3 when you have premium?
                case "Spotify Premium":
                    UpdateSpotifyState(SpotifyState.Paused);
                    break;
                // Advertisment Playing
                case "Advertisement":
                    UpdateSpotifyState(SpotifyState.PlayingAdvertisement);
                    break;
                // Advertisment playing or Starting up
                case "Spotify":
                    if (oldWindowTitle?.Length == 0) {
                        UpdateSpotifyState(SpotifyState.StartingUp);
                    } else if (oldWindowTitle == null) {
                        if (MainWindowProcess is null)
                            throw new IllegalStateException();

                        // check how long spotify has been running
                        if ((DateTime.Now - MainWindowProcess.StartTime) < TimeSpan.FromMilliseconds(3000)) {
                            UpdateSpotifyState(SpotifyState.StartingUp);
                        } else {
                            UpdateSpotifyState(SpotifyState.PlayingAdvertisement);
                        }
                    } else {
                        UpdateSpotifyState(SpotifyState.PlayingAdvertisement);
                    }
                    break;
                // Shutting down
                case "":
                    if (oldWindowTitle is null)
                        UpdateSpotifyState(SpotifyState.StartingUp);
                    else
                        UpdateSpotifyState(SpotifyState.ShuttingDown);
                    break;
                // Song Playing: "[artist] - [title]"
                case var name when name?.Contains(" - ") == true:
                    var (artist, title) = name.Split(" - ", maxCount: 2).Select(e => e.Trim()).ToArray();
                    UpdateSpotifyState(SpotifyState.PlayingSong, newSong: new SongInfo(title, artist));
                    break;
                // What is happening?
                default:
                    Logger.Hook.LogWarning($"Spotify entered an unknown state. (WindowTitle={newWindowTitle})");
                    if (AssumeAdOnUnknownState) {
                        Logger.Hook.LogInfo($"Assuming WindowTitle={newWindowTitle} marks an ad.");
                        UpdateSpotifyState(SpotifyState.PlayingAdvertisement);
                    } else {
                        UpdateSpotifyState(SpotifyState.Unknown);
                    }
                    break;
            }
        }

        /// <summary>
        /// OnSpotifyClosed is called whenever spotify is unhooked.
        /// </summary>
        protected virtual void OnSpotifyClosed() {
            Logger.Hook.LogWarning("Spotify closed.");

            ClearHookData();

            IsHooked = false;

            _windowCreationEventHook.HookGlobal();

            // scan for spotify to make sure it did not start again while we were shutting down. (should happen only during debugging)
            TryHookSpotify();
        }

        /// <summary>
        /// Clears all the state associated with a hooked spotify process.
        /// </summary>
        protected void ClearHookData() {
            MainWindowProcess?.Dispose();
            _processesCache?.DisposeAll();
            _audioSession?.Dispose();

            MainWindowProcess = null;
            _processesCache = null;
            _audioSession = null;
            WindowTitle = null;
            IsHooked = false;

            _windowCreationEventHook.TryUnhook();
            _titleChangeEventHook.TryUnhook();
            _windowDestructionEventHook.TryUnhook();
        }

        /// <inheritdoc/>
        public bool SetMute(bool mute) {
            if (!IsHooked)
                return false;

            // ensure audio session
            if (AudioSession is null) {
                Logger.Hook.LogError($"Failed to {(mute ? "mute" : "unmute")} spotify due to missing audio session.");
                return false;
            }

            // mute
            try {
                AudioSession.IsMuted = mute;
                Logger.Hook.LogInfo($"Spotify {(mute ? "muted" : "unmuted")}.");
                return true;
            } catch (Exception e) {
                Logger.Hook.LogException($"Failed to {(mute ? "mute" : "unmute")} spotify:", e);
                return false;
            }
        }

        protected override void Dispose(bool disposing) {
            if (!disposing)
                return;

            MainWindowProcess?.Dispose();
            _processesCache?.DisposeAll();

            AudioSession?.Dispose();

            _windowCreationEventHook?.Dispose();
            _titleChangeEventHook?.Dispose();
            _windowDestructionEventHook?.Dispose();
        }
    }
}
