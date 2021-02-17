using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EZBlocker3.Extensions;
using EZBlocker3.Interop;
using EZBlocker3.Logging;
using EZBlocker3.Settings;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Spotify {
    public class SkippingSpotifyAdBlocker : AbstractSpotifyAdBlocker {
        private SongInfo? lastActiveSong;

        public SkippingSpotifyAdBlocker(ISpotifyHook hook) : base(hook) { }

        protected override void OnSpotifyStateChanged(object sender, SpotifyStateChangedEventArgs e) {
            base.OnSpotifyStateChanged(sender, e);

            if (e.NewState == SpotifyState.PlayingAdvertisement) {
                Logger.AdSkipper.LogInfo("Starting to skip ad");
                Task.Run(() => KillAndRestartSpotifyAsync());
            }
        }
        protected override void OnActiveSongChanged(object sender, ActiveSongChangedEventArgs e) {
            base.OnActiveSongChanged(sender, e);

            if (e.NewActiveSong != null)
                lastActiveSong = e.NewActiveSong;
        }

        private async Task KillAndRestartSpotifyAsync() {
            Logger.AdSkipper.LogInfo("Killing spotify");
            await KillSpotifyAsync().ConfigureAwait(false);
            Logger.AdSkipper.LogInfo("Restarting spotify");
            RestartSpotify();
        }

        private static async Task KillSpotifyAsync() {
            var processes = SpotifyUtils.GetSpotifyProcesses().ToArray();

            // Close all spotify windows
            foreach (var process in processes) {
                foreach (HWND windowHandle in NativeUtils.GetAllWindowsOfProcess(process)) {
                    // sequence of relevants messages sent during shutdown induced by "Exit" entry in context menu when minimized to tray.
                    try {
                        NativeUtils.CloseWindow(windowHandle);
                        NativeUtils.DestroyWindow(windowHandle);
                    } catch (Win32Exception) { }
                    // PInvoke.SendMessage(windowHandle, Constants.WM_CLOSE, default, default);
                    // PInvoke.SendMessage(windowHandle, Constants.WM_QUIT, default, default); // does the trick if not in tray
                    // PInvoke.SendMessage(windowHandle, Constants.WM_DESTROY, default, default);
                    // PInvoke.SendMessage(windowHandle, Constants.WM_NCDESTROY, default, default); // is needed when minimized to tray
                }
            }

            // we need to wait for all processes to exit or otherwise spotify will show an error when we try to start it again.
            using var cancellationSource = new CancellationTokenSource();
            var cancellationToken = cancellationSource.Token;
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            var processesExitedTask = Task.WhenAll(processes.Select(p => p.WaitForExitAsync(cancellationToken)));
            if (await Task.WhenAny(timeoutTask, processesExitedTask).ConfigureAwait(false) == timeoutTask) {
                Logger.AdSkipper.LogInfo("Spotify did not shut down - kill it with fire! (=> .Kill())");
                // some processes did not shut down in time -> kill them
                foreach (var process in processes) {
                    if (!process.HasExited)
                        process.Kill();
                }
            }
            // ensure all tasks finish
            cancellationSource.Cancel();
        }

        private void RestartSpotify() {
            Hook.SpotifyStateChanged += Handler1;

            StartWithSpotify.StartSpotify(ignoreProxy: false);

            // TODO simplify or find better names
            void Handler1(object sender, EventArgs _) {
                if (Hook.State == SpotifyState.Paused) {
                    Hook.SpotifyStateChanged -= Handler1;
                    var process = SpotifyUtils.GetMainSpotifyProcess();
                    var windowHandle = NativeUtils.GetMainWindowOfProcess(process!);

                    Task.Run(async () => {
                        await Task.Delay(1000).ConfigureAwait(false); // if we do not wait here spotify wont update the window title and we wont detect a state change.

                        void Handler2(object sender, EventArgs _) {
                            if (Hook.State == SpotifyState.PlayingSong && windowHandle != IntPtr.Zero) {
                                Hook.SpotifyStateChanged -= Handler2;
                                if (Hook.ActiveSong is SongInfo current && lastActiveSong is SongInfo previous && current == previous) {
                                    Logger.AdSkipper.LogInfo("Previous song was resumed - skipping to next track");
                                    PInvoke.SendMessage((HWND)windowHandle, Constants.WM_APPCOMMAND, default, (LPARAM)(IntPtr)SpotifyAppCommands.NextTrack);
                                }
                            }
                        }
                        Hook.SpotifyStateChanged += Handler2;

                        Logger.AdSkipper.LogInfo("Resumed playback");
                        PInvoke.SendMessage((HWND)windowHandle, Constants.WM_APPCOMMAND, default, (LPARAM)(IntPtr)SpotifyAppCommands.PlayPause);
                    });
                }
            }
        }

        public enum SpotifyAppCommands : int {
            Mute = 0x80000,
            VolumeDown = 0x90000,
            VolumeUp = 0xA0000,
            NextTrack = 0xB0000,
            PreviousTrack = 0xC0000,
            Stop = 0xD0000,
            PlayPause = 0xE0000
        }
    }
}
