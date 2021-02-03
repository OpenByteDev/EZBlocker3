namespace EZBlocker3.Audio.Com {
    /// <summary>
    /// Defines constants that indicate the role that the system has assigned to an audio endpoint device.
    /// </summary>
    /// <remarks>
    /// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd370842.aspx
    /// </remarks>
    public enum ERole {
        /// <summary>
        /// Games, system notification sounds, and voice commands.
        /// </summary>
        eConsole = 0,

        /// <summary>
        /// Music, movies, narration, and live music recording.
        /// </summary>
        eMultimedia = 1,

        /// <summary>
        /// Voice communications (talking to another person).
        /// </summary>
        eCommunications = 2
    }
}
