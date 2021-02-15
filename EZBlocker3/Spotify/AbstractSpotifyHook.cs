using System;
using EZBlocker3.Logging;

namespace EZBlocker3.Spotify {
    public abstract class AbstractSpotifyHook : ISpotifyHook {
        private bool isActive;
        /// <summary>
        /// Gets a value indicating whether this object is active and trying to hook spotify.
        /// </summary>
        public bool IsActive {
            get => isActive;
            protected set {
                isActive = value;
                if (!isActive) {
                    IsHooked = false;
                }
            }
        }

        private bool isHooked;
        /// <summary>
        /// Gets a value indicating whether spotify is currently hooked.
        /// </summary>
        public bool IsHooked {
            get => isHooked;
            protected set {
                var oldValue = isHooked;
                isHooked = value;
                if (!isHooked) {
                    State = SpotifyState.Unknown;
                    ActiveSong = null;
                }
                if (oldValue != value) {
                    RaiseHookChanged();
                }
            }
        }

        /// <summary>
        /// Gets the currently playing song or null if no song is being played or spotify is not running.
        /// </summary>
        public SongInfo? ActiveSong { get; private set; }
        /// <summary>
        /// Gets a value indicating the current state of a running spotify process.
        /// </summary>
        public SpotifyState State { get; private set; } // = SpotifyState.Unknown;

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
        /// Occurs whenever the currently playing song changes.
        /// </summary>
        public event EventHandler<ActiveSongChangedEventArgs>? ActiveSongChanged;
        /// <summary>
        /// Occurs whenever a new spotify process is hooked or an exisiting one is unhooked.
        /// </summary>
        public event EventHandler<EventArgs>? HookChanged;
        /// <summary>
        /// Occurs whenever spotify changes its state.
        /// </summary>
        public event EventHandler<SpotifyStateChangedEventArgs>? SpotifyStateChanged;

        /// <inheritdoc/>
        public abstract void Activate();

        /// <inheritdoc/>
        public abstract void Deactivate();

        /// <summary>
        /// Updates the current state of the spotify process.
        /// </summary>
        /// <param name="newState">The new state of the spotify process.</param>
        /// <param name="newSong">The currently playing song, if applicable.</param>
        protected void UpdateSpotifyState(SpotifyState newState, SongInfo? newSong = null, bool forceRaiseEvents = false) {
            var prevSong = ActiveSong;
            var prevState = State;
            ActiveSong = newSong;
            State = newState;

            if (forceRaiseEvents || prevState != newState)
                RaiseSpotifyStateChanged(prevState, newState);
            if (forceRaiseEvents || !Equals(prevSong, newSong))
                RaiseActiveSongChanged(prevSong, newSong);
        }

        private void RaiseActiveSongChanged(SongInfo? previous, SongInfo? current) =>
            OnActiveSongChanged(new ActiveSongChangedEventArgs(previous, current));
        /// <summary>
        /// OnActiveSongChanged is called whenever the currently active song changes.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected virtual void OnActiveSongChanged(ActiveSongChangedEventArgs eventArgs) {
            Logger.Hook.LogInfo($"Active song: \"{eventArgs.NewActiveSong}\"");
            ActiveSongChanged?.Invoke(this, eventArgs);
        }

        private void RaiseHookChanged() =>
           OnHookChanged(EventArgs.Empty);
        /// <summary>
        /// OnHookChanged is called whenever spotify is hooked or unhooked.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected virtual void OnHookChanged(EventArgs eventArgs) {
            Logger.Hook.LogInfo($"Spotify {(IsHooked ? "hooked" : "unhooked")}.");
            HookChanged?.Invoke(this, eventArgs);
        }

        private void RaiseSpotifyStateChanged(SpotifyState previous, SpotifyState current) =>
            OnSpotifyStateChanged(new SpotifyStateChangedEventArgs(previous, current));
        /// <summary>
        /// OnSpotifyStateChanged is called whenever the current state of spotify changes.
        /// </summary>
        /// <param name="eventArgs"></param>
        protected virtual void OnSpotifyStateChanged(SpotifyStateChangedEventArgs eventArgs) {
            Logger.Hook.LogInfo($"Spotify is in {eventArgs.NewState} state.");
            SpotifyStateChanged?.Invoke(this, eventArgs);
        }

        #region IDisposable
        protected virtual void Dispose(bool disposing) { }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
