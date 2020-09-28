using System;
using System.ComponentModel;
using System.Text;

namespace EZBlocker3.Interop {
    internal static class NativeWindowUtils {

        public static string GetWindowTitle(IntPtr handle) {
            var titleLength = NativeMethods.GetWindowTextLength(handle);
            var builder = new StringBuilder(titleLength + 1);
            if (NativeMethods.GetWindowText(handle, builder, builder.Capacity) == 0)
                throw new Win32Exception();
            return builder.ToString();
        }

    }
}
