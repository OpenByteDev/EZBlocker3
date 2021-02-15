using System;
using System.Runtime.InteropServices;

namespace EZBlocker3.Interop {
    internal static partial class PInvokeExtra {
        /// <summary>
        /// Retrieves information about the specified window.
        /// The function also retrieves the value at a specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
        /// <param name="nIndex">The zero-based offset to the value to be retrieved.</param>
        /// <returns>
        /// If the function succeeds, the return value is the requested value.
        /// If the function fails, the return value is zero.
        /// </returns>
        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
    }
}
