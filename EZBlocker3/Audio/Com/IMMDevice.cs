using System;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.Com {
    /// <summary>
    /// Represents an audio device.
    /// </summary>
    /// <remarks>
    /// MSDN Reference: http://msdn.microsoft.com/en-us/library/dd371395.aspx
    /// </remarks>
    [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public partial interface IMMDevice {
        /// <summary>
        /// Creates a COM object with the specified interface.
        /// </summary>
        /// <param name="interfaceId">The interface identifier.</param>
		/// <param name="classContext">The execution context, defined by the COM CLSCTX enumeration.</param>
        /// <param name="activationParams">Set to NULL to activate Core Audio APIs.</param>
		/// <param name="instancePtr">The address of the interface instance specified by parameter IID.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int Activate(
            [In][MarshalAs(UnmanagedType.LPStruct)] Guid interfaceId,
            [In][MarshalAs(UnmanagedType.U4)] uint classContext,
            [In, Optional] IntPtr activationParams, // PROPVARIANT
            [Out][MarshalAs(UnmanagedType.IUnknown)] out object instancePtr);

        /*
        /// <summary>
        /// Gets an interface to the device's property store.
        /// </summary>
        /// <param name="accessMode">The <see cref="STGM"/> constant that indicates the storage mode.</param>
        /// <param name="properties">The device's property store.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        /// <remarks>
        /// Note that a client which is not running as administrator is restricted to read-only access.
        /// </remarks>
        [PreserveSig]
        int OpenPropertyStore(
            [In][MarshalAs(UnmanagedType.U4)] UInt32 accessMode,
            [Out][MarshalAs(UnmanagedType.Interface)] out IPropertyStore properties);
        */
        void NotImpl0();

        /// <summary>
        /// Retrieves an endpoint ID string that identifies the audio endpoint device.
        /// </summary>
        /// <param name="strId">The endpoint device ID.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetId(
            [Out][MarshalAs(UnmanagedType.LPWStr)] out string strId);

        /// <summary>
        /// Gets the current state of the device.
        /// </summary>
        /// <param name="deviceState">The <see cref="DEVICE_STATE_XXX"/> constant that indicates the current state.</param>
        /// <returns>An HRESULT code indicating whether the operation passed of failed.</returns>
        [PreserveSig]
        int GetState(
            [Out][MarshalAs(UnmanagedType.U4)] out uint deviceState);
    }
}
