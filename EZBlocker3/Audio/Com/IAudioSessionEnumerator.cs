using System;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.Com {
	/// <summary>
	/// Enumerates audio sessions on an audio device.
	/// </summary>
	/// <remarks>
	/// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd368281.aspx
	/// </remarks>
	[Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public partial interface IAudioSessionEnumerator {
		/// <summary>
		/// Gets the total number of audio sessions that are open on the audio device.
		/// </summary>
		/// <param name="count">Receives the total number of audio sessions.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetCount(
			[Out][MarshalAs(UnmanagedType.I4)] out int count);

		/// <summary>
		/// Gets the audio session specified by an audio session number.
		/// </summary>
		/// <param name="index">The zero-based index of the session.</param>
		/// <param name="session">Receives an <see cref="IAudioSessionControl2"/> session object in the collection that is maintained by the session enumerator.</param>
		/// <returns>An HRESULT code indicating whether the operation succeeded of failed.</returns>
		[PreserveSig]
		int GetSession(
			[In][MarshalAs(UnmanagedType.I4)] int index,
			[Out][MarshalAs(UnmanagedType.Interface)] out IAudioSessionControl session);
	}
}
