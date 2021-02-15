using System;
using System.Linq;
using EZBlocker3.Extensions;
using EZBlocker3.Logging;
using Windows.Media.Control;
using Manager = Windows.Media.Control.GlobalSystemMediaTransportControlsSessionManager;
using Session = Windows.Media.Control.GlobalSystemMediaTransportControlsSession;

namespace EZBlocker3.Spotify {
    public class GlobalSystemMediaTransportControlSpotifyHook : AbstractSpotifyHook {
        private Manager? manager;
        private Session? session;

        public override void Activate() {
            if (IsActive)
                throw new InvalidOperationException("Hook is already active.");

            Logger.Hook.LogDebug("Activated");

            IsActive = true;

            // awaiting here hangs the app?
            manager = Manager.RequestAsync().AsTask().GetAwaiter().GetResult();

            SetupHook();
            TryHook();
        }

        public override void Deactivate() {
            if (!IsActive)
                throw new InvalidOperationException("Hook has to be active.");

            IsActive = false;

            ClearHook();

            Logger.Hook.LogDebug("Deactivated");
        }

        private bool TryHook() {
            if (IsHooked)
                return true;

            var sessions = manager!.GetSessions().ToArray();

            var exactMatch = sessions.Where(e => e.SourceAppUserModelId.Equals("Spotify.exe", StringComparison.OrdinalIgnoreCase));
            var fuzzyMatch = sessions.Where(e => e.SourceAppUserModelId.Contains("spotify", StringComparison.OrdinalIgnoreCase));

            var spotifySession = exactMatch.Concat(fuzzyMatch).FirstOrDefault();

            if (spotifySession == null)
                return false;

            HookSession(spotifySession);
            return true;
        }

        private void SetupHook() {
            manager!.SessionsChanged += Manager_SessionsChanged;
        }

        private void ClearHook() {
            if (manager != null) {
                manager.SessionsChanged -= Manager_SessionsChanged;
            }
            session = null;
        }

        private void Manager_SessionsChanged(object sender, SessionsChangedEventArgs args) {
            if (IsHooked) {
                if (!manager!.GetSessions().Contains(session)) {
                    if (session != null) {
                        session.MediaPropertiesChanged -= SpotifySession_MediaPropertiesChanged;
                        session.PlaybackInfoChanged -= SpotifySession_PlaybackInfoChanged;
                        session = null;
                    }
                    IsHooked = false;
                    TryHook();
                }
            } else {
                TryHook();
            }
        }

        private void HookSession(Session spotifySession) {
            Logger.Hook.LogInfo("Hooked Session");
            session = spotifySession;

            session.MediaPropertiesChanged += SpotifySession_MediaPropertiesChanged;
            session.PlaybackInfoChanged += SpotifySession_PlaybackInfoChanged;

            IsHooked = true;
            HandleSpotifyStateChanged();
        }

        private void SpotifySession_PlaybackInfoChanged(object sender, PlaybackInfoChangedEventArgs args) {
            HandleSpotifyStateChanged();
        }

        private void SpotifySession_MediaPropertiesChanged(object sender, MediaPropertiesChangedEventArgs args) {
            HandleSpotifyStateChanged();
        }

        private void HandleSpotifyStateChanged() {
            try {
                var mediaProperties = session.TryGetMediaPropertiesAsync().AsTask().GetAwaiter().GetResult();
                var playbackInfo = session.GetPlaybackInfo();

                var title = mediaProperties.Title;
                var artist = mediaProperties.Artist;

                Logger.Hook.LogDebug($"Media Properties: (Title: \"{title}\", Artist: \"{artist}\")");
                Logger.Hook.LogDebug($"PlaybackInfo: (Status: \"{playbackInfo.PlaybackStatus}\")");

                var isEmptyMedia = string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(artist);
                var isAd = artist == "Spotify" || artist == "Sponsored Message" || title == "Advertisement" || title == "Spotify";

                var state = playbackInfo.PlaybackStatus switch {
                    GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing =>
                        isAd ? SpotifyState.PlayingAdvertisement : (isEmptyMedia ? State : SpotifyState.PlayingSong),
                    GlobalSystemMediaTransportControlsSessionPlaybackStatus.Paused => SpotifyState.Paused,
                    GlobalSystemMediaTransportControlsSessionPlaybackStatus.Opened => SpotifyState.StartingUp,
                    GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed => SpotifyState.ShuttingDown,
                    _ => SpotifyState.Unknown
                };

                var song = !isAd && !isEmptyMedia ? new SongInfo(title, artist) : null;

                UpdateSpotifyState(state, song);
            } catch (Exception e) {
                Logger.Hook.LogException("Error while trying to determined spotify state", e);
            }
        }
    }
}
