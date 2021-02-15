using System.Threading.Tasks;

namespace EZBlocker3.Spotify {
    public class MutingSpotifyAdBlocker : AbstractSpotifyAdBlocker {
        private readonly IMutingSpotifyHook muter;

        // TODO
        public bool WaitForAudioFade { get; set; } = true;
        public bool AggressiveMuting { get; set; } = false;

        public MutingSpotifyAdBlocker(ProcessAndWindowEventSpotifyHook hook) : this(hook, hook) { }
        public MutingSpotifyAdBlocker(ISpotifyHook hook, IMutingSpotifyHook muter) : base(hook) {
            this.muter = muter;
        }

        protected override void OnSpotifyStateChanged(object sender, SpotifyStateChangedEventArgs eventArgs) {
            var oldState = eventArgs.PreviousState;
            var newState = eventArgs.NewState;

            if (newState == SpotifyState.StartingUp || newState == SpotifyState.ShuttingDown)
                return;

            // we skip here as no audio session is present and muting would fail.
            if (oldState == SpotifyState.StartingUp && newState == SpotifyState.Paused)
                return;

            if (AggressiveMuting) {
                muter.SetMute(newState != SpotifyState.PlayingSong);
                return;
            }

            if (!WaitForAudioFade || oldState != SpotifyState.PlayingAdvertisement) {
                muter.SetMute(mute: newState == SpotifyState.PlayingAdvertisement);
                return;
            }

            if (newState == SpotifyState.PlayingAdvertisement) {
                muter.Mute();
                return;
            }

            Task.Run(async () => {
                /*
                for (var i = 0; i < 10; i++) {
                    await Task.Delay(50);
                    var peakVolume = SpotifyHook.AudioSession?.PeakVolume;
                    Logging.Logger.LogDebug("peakVolume: " + peakVolume);
                    if (peakVolume is null)
                        break;
                    if (peakVolume < 0.01)
                        break;
                }*/
                await Task.Delay(600).ConfigureAwait(false);
                muter.Unmute();
            });
        }
    }
}
