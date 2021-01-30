using System.Threading.Tasks;
using static EZBlocker3.Spotify.SpotifyHook;

namespace EZBlocker3.Spotify {
    public class SpotifyMuter {
        public SpotifyHook SpotifyHook { get; private init; }

        public bool WaitForAudioFade { get; set; } = true;

        public SpotifyMuter(SpotifyHook hook) {
            SpotifyHook = hook;
            SpotifyHook.SpotifyStateChanged += OnSpotifyStateChanged;
        }

        protected virtual void OnSpotifyStateChanged(object sender, SpotifyStateChangedEventArgs eventArgs) {
            var oldState = eventArgs.PreviousState;
            var newState = eventArgs.NewState;

            if (newState == SpotifyState.StartingUp || newState == SpotifyState.ShuttingDown)
                return;

            // we skip here as no audio session is present and muting would fail.
            if (oldState == SpotifyState.StartingUp && newState == SpotifyState.Paused)
                return;

            if (oldState == SpotifyState.PlayingAdvertisement) {
                SpotifyHook.Unmute();
                return;
            }

            if (!WaitForAudioFade) {
                SpotifyHook.SetMute(mute: SpotifyHook.IsAdPlaying);
                return;
            }

            if (SpotifyHook.IsAdPlaying) {
                SpotifyHook.Mute();
                return;
            }

            Task.Run(async () => {
                for (var i = 0; i < 10; i++) {
                    await Task.Delay(50);
                    var peakVolume = SpotifyHook.AudioSession?.PeakVolume;
                    if (peakVolume is null)
                        break;
                    if (peakVolume == 0) {
                        SpotifyHook.Unmute();
                        break;
                    }
                }
            });
        }
    }
}
