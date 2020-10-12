namespace EZBlocker3.Audio.Com {
	/// <summary>
	/// Defines constants that indicate the direction in which audio data flows between an audio endpoint device and an application.
	/// </summary>
	/// <remarks>
	/// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd370828.aspx
	/// </remarks>
	public enum EDataFlow {
		/// <summary>
		/// Audio data flows from the application to the audio endpoint device, which renders the stream.
		/// </summary>
		eRender = 0,

		/// <summary>
		/// Audio data flows from the audio endpoint device that captures the stream, to the application.
		/// </summary>
		eCapture = 1,

		/// <summary>
		/// Audio data can flow either from the application to the audio endpoint device, or from the audio endpoint device to the application.
		/// </summary>
		eAll = 2
	}
}
