using System.Threading.Tasks;
using static EZBlocker3.SpotifyHook;

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

            if (oldState == SpotifyState.PlayingAdvertisement) {
                SpotifyHook.Unmute();
                return;
            }

            if (WaitForAudioFade)
                Task.Run(async () => {
                    await Task.Delay(300);
                    SpotifyHook.SetMute(mute: SpotifyHook.IsAdPlaying);
                });
            else
                SpotifyHook.SetMute(mute: SpotifyHook.IsAdPlaying);
        }

    }
}
