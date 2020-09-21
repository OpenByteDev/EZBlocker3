using System;
using System.Runtime.InteropServices;

namespace EZBlocker3.Interop {
    public static class Winuser {

        /// <summary>
        /// Flashes the specified window. It does not change the active state of the window.
        /// </summary>
        /// <param name="pwfi">A pointer to a <see cref="FLASHWINFO"/> structure.</param>
        /// <returns>
        /// The return value specifies the window's state before the call to the FlashWindowEx function.
        /// If the window caption was drawn as active before the call, the return value is nonzero.
        /// Otherwise, the return value is zero.
        /// </returns>
        /// <see cref="FLASHWINFO" />
        /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-flashwindowex"/>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        /// <summary>
        /// Contains the flash status for a window and the number of times the system should flash the window.
        /// </summary>
        /// <see cref="FlashWindowEx(ref FLASHWINFO)" />
        /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/ns-winuser-flashwinfo"/>
        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO {
            /// <summary>
            /// The size of the structure, in bytes.
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// A handle to the window to be flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// The flash status.
            /// </summary>
            public FlashWindowFlags dwFlags;
            /// <summary>
            /// The number of times to flash the window.
            /// </summary>
            public uint uCount;
            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If 0, the function uses the default cursor blink rate.
            /// </summary>
            public uint dwTimeout;
        }

        /// <summary>
        /// Flags for the <see cref="FLASHWINFO.dwFlags"/> field.
        /// </summary>
        [Flags]
        public enum FlashWindowFlags : uint {
            /// <summary>
            /// Stop flashing. The system restores the window to its original stae.
            /// </summary>
            FLASHW_STOP = 0,
            /// <summary>
            /// Flash the window caption.
            /// </summary>
            FLASHW_CAPTION = 1,
            /// <summary>
            /// Flash the taskbar button.
            /// </summary>
            FLASHW_TRAY = 2,
            /// <summary>
            /// Flash both the window caption and taskbar button.
            /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
            /// </summary>
            FLASHW_ALL = 3,
            /// <summary>
            /// Flash continuously, until the FLASHW_STOP flag is set.
            /// </summary>
            FLASHW_TIMER = 4,
            /// <summary>
            /// Flash continuously until the window comes to the foreground.
            /// </summary>
            FLASHW_TIMERNOFG = 12
        }

    }
}
