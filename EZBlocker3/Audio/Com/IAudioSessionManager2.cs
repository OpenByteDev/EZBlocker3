using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.Com {
    /// <summary>
    /// Enables an application to manage submixes for the audio device.
    /// </summary>
    /// <remarks>
    /// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd370950.aspx
    /// </remarks>
    [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IAudioSessionManager2 {
        // Note: We can't derive from IAudioSessionControl, as that will produce the wrong vtable.

        #region IAudioSessionManager Methods
        /*
		/// <summary>
		/// Retrieves an audio session control.
		/// </summary>
		/// <param name="sessionId">A new or existing session ID.</param>
		/// <param name="streamFlags">Audio session flags.</param>
		/// <param name="sessionControl">Receives an <see cref="IAudioSessionControl"/> interface for the audio session.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetAudioSessionControl(
			[In, Optional][MarshalAs(UnmanagedType.LPStruct)] Guid sessionId,
			[In][MarshalAs(UnmanagedType.U4)] UInt32 streamFlags,
			[Out][MarshalAs(UnmanagedType.Interface)] out IAudioSessionControl sessionControl);
		*/
        void NotImpl0();

        /*
		/// <summary>
		/// Retrieves a simple audio volume control.
		/// </summary>
		/// <param name="sessionId">A new or existing session ID.</param>
		/// <param name="streamFlags">Audio session flags.</param>
		/// <param name="audioVolume">Receives an <see cref="ISimpleAudioVolume"/> interface for the audio session.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetSimpleAudioVolume(
			[In, Optional][MarshalAs(UnmanagedType.LPStruct)] Guid sessionId,
			[In][MarshalAs(UnmanagedType.U4)] UInt32 streamFlags,
			[Out][MarshalAs(UnmanagedType.Interface)] out ISimpleAudioVolume audioVolume);

		#endregion
		*/
        void NotImpl1();
        #endregion

        /// <summary>
        /// Gets a pointer to the audio session enumerator object used to enumerate sessions.
        /// </summary>
        /// <param name="sessionList">Receives the session enumerator object that the client can use to enumerate audio sessions on the audio device.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int GetSessionEnumerator(
            [Out][MarshalAs(UnmanagedType.Interface)] out IAudioSessionEnumerator sessionList);

        /*
		/// <summary>
		/// Registers the application to receive a notification when a session is created.
		/// </summary>
		/// <param name="client">The client to be called when session events are raised.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int RegisterSessionNotification(
			[In] IAudioSessionNotification client);
		*/

        /*
		/// <summary>
		/// Deletes the registration to receive a notification when a session is created.
		/// </summary>
		/// <param name="client">Removes the client from the callback list for session events.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int UnregisterSessionNotification(
			[In] IAudioSessionNotification client);
		*/

        /*
		/// <summary>
		/// Registers the application to receive ducking notifications.
		/// </summary>
		/// <param name="sessionId">A session instance identifier.</param>
		/// <param name="client">The client to be called when ducking events are raised.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int RegisterDuckNotification(
			[In][MarshalAs(UnmanagedType.LPWStr)] string sessionId,
			[In] IAudioVolumeDuckNotification client);
		*/

        /*
		/// <summary>
		/// Deletes the registration to receive ducking notifications.
		/// </summary>
		/// <param name="client">Removes the client from the callback list for ducking events.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int UnregisterDuckNotification(
			[In] IAudioVolumeDuckNotification client);
		*/
    }
}
