namespace EZBlocker3.Spotify {
    public interface IMutingSpotifyHook {
        /// <summary>
        /// Sets the spotify mute status to the given state.
        /// </summary>
        /// <param name="mute">A value indicating whether spotify should be muted or unmuted.</param>
        /// <returns>A value indicating whether the operation was successful</returns>
        bool SetMute(bool mute);
    }

    public static class ISpotifyMuterExtensions {
        /// <summary>
        /// Mutes the spotify audio session.
        /// </summary>
        /// <returns>A value indicating whether the operation was successful</returns>
        public static bool Mute(this IMutingSpotifyHook muter) => muter.SetMute(true);
        /// <summary>
        /// Unmutes the spotify audio session.
        /// </summary>
        /// <returns>A value indicating whether the operation was successful</returns>
        public static bool Unmute(this IMutingSpotifyHook muter) => muter.SetMute(false);
    }
}
