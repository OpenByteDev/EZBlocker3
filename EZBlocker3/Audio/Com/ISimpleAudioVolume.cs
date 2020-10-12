using System;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.Com {
	/// <summary>
	/// Enables a client to control the master volume level of an audio session. 
	/// </summary>
	/// <remarks>
	/// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd316531.aspx
	/// </remarks>
	[Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public partial interface ISimpleAudioVolume {
		/// <summary>
		/// Sets the master volume level for the audio session.
		/// </summary>
		/// <param name="levelNorm">The new volume level expressed as a normalized value between 0.0 and 1.0.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int SetMasterVolume(
			[In][MarshalAs(UnmanagedType.R4)] float levelNorm,
			[In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

		/// <summary>
		/// Retrieves the client volume level for the audio session.
		/// </summary>
		/// <param name="levelNorm">Receives the volume level expressed as a normalized value between 0.0 and 1.0. </param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetMasterVolume(
			[Out][MarshalAs(UnmanagedType.R4)] out float levelNorm);

		/// <summary>
		/// Sets the muting state for the audio session.
		/// </summary>
		/// <param name="isMuted">The new muting state.</param>
		/// <param name="eventContext">A user context value that is passed to the notification callback.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int SetMute(
			[In][MarshalAs(UnmanagedType.Bool)] bool isMuted,
			[In][MarshalAs(UnmanagedType.LPStruct)] Guid eventContext);

		/// <summary>
		/// Retrieves the current muting state for the audio session.
		/// </summary>
		/// <param name="isMuted">Receives the muting state.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetMute(
			[Out][MarshalAs(UnmanagedType.Bool)] out bool isMuted);
	}
}
