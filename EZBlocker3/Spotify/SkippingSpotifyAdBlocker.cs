using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EZBlocker3.Extensions;
using EZBlocker3.Interop;
using EZBlocker3.Settings;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Spotify {
    public class SkippingSpotifyAdBlocker : AbstractSpotifyAdBlocker {
        public SkippingSpotifyAdBlocker(ISpotifyHook hook) : base(hook) { }

        protected override void OnSpotifyStateChanged(object sender, SpotifyStateChangedEventArgs e) {
            if (e.NewState == SpotifyState.PlayingAdvertisement) {
                Task.Run(() => KillAndRestartSpotifyAsync());
            }
        }

        private async Task KillAndRestartSpotifyAsync() {
            var processes = SpotifyProcessUtils.GetSpotifyProcesses().ToArray();

            // Close all spotify windows
            foreach (var process in processes) {
                foreach (HWND windowHandle in NativeUtils.GetAllWindowsOfProcess(process)) {
                    // sequence of relevants messages sent during shutdown induced by "Exit" entry in context menu when minimized to tray.
                    PInvoke.SendMessage(windowHandle, Constants.WM_CLOSE, default, default);
                    PInvoke.SendMessage(windowHandle, Constants.WM_QUIT, default, default); // does the trick if not in tray
                    PInvoke.SendMessage(windowHandle, Constants.WM_DESTROY, default, default);
                    PInvoke.SendMessage(windowHandle, Constants.WM_NCDESTROY, default, default); // is needed when minimized to tray
                }
            }

            // we need to wait for all processes to exit or otherwise spotify will show an error when we try to start it again.
            using var cancellationSource = new CancellationTokenSource();
            var cancellationToken = cancellationSource.Token;
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            var processesExitedTask = Task.WhenAll(processes.Select(p => p.WaitForExitAsync(cancellationToken)));
            if (await Task.WhenAny(timeoutTask, processesExitedTask).ConfigureAwait(false) == timeoutTask) {
                // some processes did not shut down in time -> kill them
                foreach (var process in processes) {
                    if (!process.HasExited)
                        process.Kill();
                }
            }
            // ensure all tasks finish
            cancellationSource.Cancel();

            Hook.SpotifyStateChanged += Handler;

            StartWithSpotify.StartSpotify();

            void Handler(object sender, EventArgs e) {
                if (Hook.State == SpotifyState.Paused) {
                    Hook.SpotifyStateChanged -= Handler;

                    var process = SpotifyProcessUtils.GetMainSpotifyProcess();
                    var windowHandle = NativeUtils.GetMainWindowOfProcess(process!);

                    Thread.Sleep(1000); // if we do not wait here spotify wont update the window title and we wont detect a state change.

                    PInvoke.SendMessage((HWND)windowHandle, Constants.WM_APPCOMMAND, default, (LPARAM)0xE0000);
                }
            }
        }
    }
}
