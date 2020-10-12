using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.Com {
    /// <summary>
    /// Represents a peak meter on the audio stream to or from an audio endpoint device.
    /// </summary>
    /// <remarks>
    /// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd368227.aspx
    /// </remarks>
    [Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IAudioMeterInformation {
        /// <summary>
        /// Gets the peak sample value for the channels in the audio stream.
        /// </summary>
        /// <param name="peak">The peak sample value for the audio stream.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetPeakValue(
            [Out][MarshalAs(UnmanagedType.R4)] out float peak);

        /// <summary>
        /// Gets the number of channels in the audio stream that are monitored by peak meters.
        /// </summary>
        /// <param name="channelCount">The channel count.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetMeteringChannelCount(
            [Out][MarshalAs(UnmanagedType.U4)] out uint channelCount);

        /// <summary>
        /// Gets the peak sample values for all the channels in the audio stream.
        /// </summary>
        /// <param name="channelCount">The channel count.</param>
        /// <param name="peakValues">An array of peak sample values.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetChannelsPeakValues(
            [In][MarshalAs(UnmanagedType.U4)] uint channelCount,
            [In, Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.R4)] float[] peakValues);

        /// <summary>
        /// Queries the audio endpoint device for its hardware-supported functions.
        /// </summary>
        /// <param name="hardwareSupportMask">A hardware support mask that indicates the capabilities of the endpoint.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int QueryHardwareSupport(
            [Out][MarshalAs(UnmanagedType.U4)] out uint hardwareSupportMask);
    }
}
