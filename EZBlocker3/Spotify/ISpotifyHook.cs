using System;

namespace EZBlocker3.Spotify {
    public interface ISpotifyHook : IActivatable, IDisposable {
        /// <summary>
        /// Gets the currently playing song or null if no song is being played or spotify is not running.
        /// </summary>
        public SongInfo? ActiveSong { get; }
        /// <summary>
        /// Gets a value indicating the current state of a running spotify process.
        /// </summary>
        public SpotifyState State { get; }
        /// <summary>
        /// Gets a value indicating the current state of a running spotify process.
        /// </summary>
        public bool IsHooked { get; }

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
    }

    #region EventArgs
    public class SpotifyStateChangedEventArgs : EventArgs {
        public SpotifyState PreviousState { get; }
        public SpotifyState NewState { get; }

        public SpotifyStateChangedEventArgs(SpotifyState previousState, SpotifyState newState) {
            PreviousState = previousState;
            NewState = newState;
        }
    }

    public class ActiveSongChangedEventArgs : EventArgs {
        public SongInfo? PreviousActiveSong { get; }
        public SongInfo? NewActiveSong { get; }

        public ActiveSongChangedEventArgs(SongInfo? previousActiveSong, SongInfo? newActiveSong) {
            PreviousActiveSong = previousActiveSong;
            NewActiveSong = newActiveSong;
        }
    }
    #endregion
}
