using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Interop {
    internal static class NativeUtils {
        public static string GetWindowTitle(IntPtr handle) {
            var titleLength = PInvoke.GetWindowTextLength((HWND)handle);
            if (titleLength == 0)
                return string.Empty;
            var title = new string('\0', titleLength);
            if (PInvoke.GetWindowText((HWND)handle, title, titleLength + 1) == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
            return title;
        }

        public static bool IsPopupWindow(IntPtr handle) {
            var style = PInvokeExtra.GetWindowLongPtr(handle, Constants.GWL_STYLE);
            return ((long)style & Constants.WS_POPUP) != 0;
        }

        public static bool IsRootWindow(IntPtr handle) {
            return PInvoke.GetWindow((HWND)handle, Constants.GW_OWNER).Value == 0;
        }

        public static uint GetWindowThreadProcessId(IntPtr windowHandle) {
            uint processId;
            unsafe {
                Marshal.ThrowExceptionForHR((int)PInvoke.GetWindowThreadProcessId((HWND)windowHandle, &processId));
            }
            return processId;
        }

        public static List<IntPtr> GetAllWindowsOfProcess(Process process) {
            var handles = new List<IntPtr>(0);

            var callback = new WNDENUMPROC((hWnd, _) => { handles.Add(hWnd); return true; });
            foreach (ProcessThread thread in process.Threads) {
                PInvoke.EnumThreadWindows((uint)thread.Id, callback, default);
                thread.Dispose();
            }

            GC.KeepAlive(handles);
            GC.KeepAlive(callback);

            return handles;
        }

        // Process.MainWindowHandle does not consider hidden windows which is why the app failed to detect spotify in the system tray.
        public static IntPtr GetMainWindowOfProcess(Process process) => GetMainWindowOfProcess((uint)process.Id);
        public static IntPtr GetMainWindowOfProcess(uint targetProcessId) {
            var mainWindowHandle = IntPtr.Zero;
            var enumerationStopped = false;

            var callback = new WNDENUMPROC(EnumWindowsCallback);
            if (!PInvoke.EnumWindows(callback, default) && !enumerationStopped)
                throw new Win32Exception();

            GC.KeepAlive(callback);

            return mainWindowHandle;

            BOOL EnumWindowsCallback(HWND handle, LPARAM _) {
                var processId = GetWindowThreadProcessId(handle);

                // belongs to correct process?
                if (processId != targetProcessId)
                    return true;

                // is root window?
                if (!IsRootWindow(handle))
                    return true;

                // has window title?
                if (PInvoke.GetWindowTextLength(handle) == 0)
                    return true;

                mainWindowHandle = handle;
                enumerationStopped = true;
                return false;
            }
        }

        public static string? GetMainWindowTitle(Process process) {
            var mainWindowHandle = GetMainWindowOfProcess(process);

            if (mainWindowHandle == IntPtr.Zero)
                return null;

            return GetWindowTitle(mainWindowHandle);
        }

        public static ReadOnlySpan<char> GetWindowClassName(IntPtr windowHandle) {
            var name = new string(' ', 256); // 256 is the max name length

            var actualNameLength = PInvoke.GetClassName((HWND)windowHandle, name, name.Length);
            if (actualNameLength == 0)
                throw new Win32Exception();

            return name.AsSpan(0, actualNameLength);
        }

        public static void CloseWindow(IntPtr windowHandle) {
            if (!PInvoke.CloseWindow((HWND)windowHandle))
                throw new Win32Exception();
        }
    }
}
