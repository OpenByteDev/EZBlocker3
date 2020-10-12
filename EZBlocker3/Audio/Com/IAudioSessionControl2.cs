﻿using System;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.Com {
    /// <summary>
    /// Used by a client to get information about the audio session.
    /// </summary>
    /// <remarks>
    /// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd368248.aspx
    /// </remarks>
    [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IAudioSessionControl2 {
        // Note: We can't derive from IAudioSessionControl, as that will produce the wrong vtable.

        #region IAudioSessionControl Methods
        /*
        /// <summary>
        /// Retrieves the current state of the audio session.
        /// </summary>
        /// <param name="state">Receives the current session state.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int GetState(
            [Out] out AudioSessionState state);
        */
        void NotImpl0();

        /// <summary>
        /// Retrieves the display name for the audio session.
        /// </summary>
        /// <param name="displayName">Receives a string that contains the display name.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int GetDisplayName(
            [Out][MarshalAs(UnmanagedType.LPWStr)] out string displayName);

        /// <summary>
        /// Assigns a display name to the current audio session.
        /// </summary>
        /// <param name="displayName">A string that contains the new display name for the session.</param>
        /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int SetDisplayName(
            [In][MarshalAs(UnmanagedType.LPWStr)] string displayName,
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

        /// <summary>
        /// Retrieves the path for the display icon for the audio session.
        /// </summary>
        /// <param name="iconPath">Receives a string that specifies the fully qualified path of the file that contains the icon.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int GetIconPath(
            [Out][MarshalAs(UnmanagedType.LPWStr)] out string iconPath);

        /// <summary>
        /// Assigns a display icon to the current session.
        /// </summary>
        /// <param name="iconPath">A string that specifies the fully qualified path of the file that contains the new icon.</param>
        /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int SetIconPath(
            [In][MarshalAs(UnmanagedType.LPWStr)] string iconPath,
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

        /// <summary>
        /// Retrieves the grouping parameter of the audio session.
        /// </summary>
        /// <param name="groupingId">Receives the grouping parameter ID.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int GetGroupingParam(
            [Out] out Guid groupingId);

        /// <summary>
        /// Assigns a session to a grouping of sessions.
        /// </summary>
        /// <param name="groupingId">The new grouping parameter ID.</param>
        /// <param name="eventContext">A user context value that is passed to the notification callback.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int SetGroupingParam(
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid groupingId,
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

        /*
        /// <summary>
        /// Registers the client to receive notifications of session events, including changes in the session state.
        /// </summary>
        /// <param name="client">A client-implemented <see cref="IAudioSessionEvents"/> interface.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int RegisterAudioSessionNotification(
            [In] IAudioSessionEvents client);
        */
        void NotImpl1();

        /*
        /// <summary>
        /// Deletes a previous registration by the client to receive notifications.
        /// </summary>
        /// <param name="client">A client-implemented <see cref="IAudioSessionEvents"/> interface.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int UnregisterAudioSessionNotification(
            [In] IAudioSessionEvents client);
        */
        void NotImpl2();

        #endregion

        /// <summary>
		/// Retrieves the session identifier.
		/// </summary>
		/// <param name="sessionId">Receives the audio session identifier.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
        int GetSessionIdentifier(
            [Out][MarshalAs(UnmanagedType.LPWStr)] out string sessionId);

        /// <summary>
        /// Retrieves the identifier of the session instance.
        /// </summary>
        /// <param name="instanceId">Receives the identifier of a particular instance of the audio session.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int GetSessionInstanceIdentifier(
            [Out][MarshalAs(UnmanagedType.LPWStr)] out string instanceId);

        /// <summary>
        /// Retrieves the process identifier of the session.
        /// </summary>
        /// <param name="processId">Receives the process identifier of the audio session. </param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int GetProcessId(
            [Out][MarshalAs(UnmanagedType.U4)] out uint processId);

        /// <summary>
        /// Indicates whether the session is a system sounds session.
        /// </summary>
        /// <returns>An HRESULT code returning S_OK (0x0) or S_FALSE (0x1), indicating whether or not the session is a system sounds session.</returns>
        [PreserveSig]
        int IsSystemSoundsSession();

        /// <summary>
        /// Enables or disables the default stream attenuation experience (auto-ducking) provided by the system.
        /// </summary>
        /// <param name="optOut">True to disable system auto-ducking, or false to enable.</param>
        /// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
        [PreserveSig]
        int SetDuckingPreference(
            [In][MarshalAs(UnmanagedType.Bool)] bool optOut);
    }
}
