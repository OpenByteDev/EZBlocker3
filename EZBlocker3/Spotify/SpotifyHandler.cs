using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Spotify {
    public sealed class SpotifyHandler : IActivatable, IDisposable {
        public ISpotifyHook Hook { get; }
        public IMutingSpotifyHook? Muter { get; }
        public IActivatable? AdBlocker { get; }
        public bool IsActive { get; private set; }
        private readonly CancellationTokenSource cancellationTokenSource = new();

        public SpotifyHandler() {
            switch (Properties.Settings.Default.Hook) {
                case nameof(GlobalSystemMediaTransportControlSpotifyHook):
                    Hook = new GlobalSystemMediaTransportControlSpotifyHook();
                    break;
                // case nameof(ProcessAndWindowEventSpotifyHook):
                default:
                    var hook = new ProcessAndWindowEventSpotifyHook() {
                        AssumeAdOnUnknownState = Properties.Settings.Default.AggressiveMuting
                    };
                    Hook = hook;
                    Muter = hook;
                    break;
            }

            AdBlocker = (Properties.Settings.Default.BlockType, Hook) switch {
                (nameof(MutingSpotifyAdBlocker), IMutingSpotifyHook muter) => new MutingSpotifyAdBlocker(Hook, muter) {
                    AggressiveMuting = Properties.Settings.Default.AggressiveMuting
                },
                // (nameof(SkippingSpotifyAdBlocker), _) => 
                _ => new SkippingSpotifyAdBlocker(Hook),
            };
        }

        public void Activate() {
            Task.Run(() => {
                AdBlocker?.Activate();
                Hook.Activate();

                while (!cancellationTokenSource.IsCancellationRequested) {
                    var res = PInvoke.GetMessage(out var msg, default, 0, 0);

                    if (!res)
                        break;

                    PInvoke.TranslateMessage(msg);
                    PInvoke.DispatchMessage(msg);
                }
            }, cancellationTokenSource.Token);

            IsActive = true;
        }

        public void Deactivate() {
            AdBlocker?.Deactivate();
            Hook.Deactivate();

            IsActive = false;
        }

        public void Dispose() {
            Hook.Dispose();
        }
    }
}
