using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace EZBlocker3.Interop {
    internal static class NativeWindowUtils {
        public static string GetWindowTitle(IntPtr handle) {
            var titleLength = NativeMethods.GetWindowTextLength(handle);
            if (titleLength == 0)
                return string.Empty;
            var builder = new StringBuilder(titleLength + 1);
            if (NativeMethods.GetWindowText(handle, builder, builder.Capacity) == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return builder.ToString();
        }
    }
}
