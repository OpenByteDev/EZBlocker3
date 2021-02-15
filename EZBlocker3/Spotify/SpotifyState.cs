namespace EZBlocker3.Spotify {
    /// <summary>
    /// Represents the current state of a running spotify process.
    /// </summary>
    public enum SpotifyState {
        /// <summary>
        /// Spotify is in an unknown state.
        /// </summary>
        Unknown,
        /// <summary>
        /// Spotify is playing a song.
        /// </summary>
        PlayingSong,
        /// <summary>
        /// Spotify is playing an advertisement.
        /// </summary>
        PlayingAdvertisement,
        /// <summary>
        /// Spotify is paused.
        /// </summary>
        Paused,
        /// <summary>
        /// Spotify is in the process of starting up.
        /// </summary>
        StartingUp,
        /// <summary>
        /// Spotify is in the process of shutting down.
        /// </summary>
        ShuttingDown
    }
}
