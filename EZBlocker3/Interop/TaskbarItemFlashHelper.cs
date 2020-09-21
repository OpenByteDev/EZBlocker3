using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using static EZBlocker3.Interop.Winuser;

namespace EZBlocker3.Interop {
    // based on https://stackoverflow.com/a/8929473/6304917
    internal static class TaskbarItemFlashHelper {

        private static IntPtr? _mainWindowHandle;
        private static IntPtr MainWindowHandle => _mainWindowHandle ??= GetMainWindowHandle();

        public static void FlashMainWindowUntilFocused(uint count = uint.MaxValue, uint interval = default) {
            FlashUntilFocused(MainWindowHandle, count, interval);
        }
        public static bool FlashUntilFocused(Window window, uint count = uint.MaxValue, uint interval = default) {
            var handle = new WindowInteropHelper(window).Handle;
            return FlashUntilFocused(handle, count, interval);
        }
        public static bool FlashUntilFocused(IntPtr windowHandle, uint count = uint.MaxValue, uint interval = default) {
            var flashInfo = CreateFlashInfoStruct(windowHandle, FlashWindowFlags.FLASHW_TIMERNOFG | FlashWindowFlags.FLASHW_ALL, count, interval);
            return FlashWindowEx(ref flashInfo);
        }

        private static FLASHWINFO CreateFlashInfoStruct(IntPtr windowHandle, FlashWindowFlags flags, uint count, uint interval) {
            return new FLASHWINFO() {
                cbSize = Convert.ToUInt32(Marshal.SizeOf(typeof(FLASHWINFO))),
                hwnd = windowHandle,
                dwFlags = flags,
                uCount = count,
                dwTimeout = interval
            };
        }
        public static void StopMainWindowFlashing() {
            StopFlashing(MainWindowHandle);
        }
        public static bool StopFlashing(Window window) {
            var handle = new WindowInteropHelper(window).Handle;
            return StopFlashing(handle);
        }
        public static bool StopFlashing(IntPtr windowHandle) {
            var flashInfo = CreateFlashInfoStruct(windowHandle, FlashWindowFlags.FLASHW_STOP, default, default);
            return FlashWindowEx(ref flashInfo);
        }

        private static IntPtr GetMainWindowHandle() {
            var mainWindow = Application.Current.MainWindow;
            return new WindowInteropHelper(mainWindow).Handle;
        }
    }
}
