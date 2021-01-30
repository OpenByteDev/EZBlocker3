using System;
using System.Runtime.InteropServices;
using System.Text;

namespace EZBlocker3.Interop {
    internal static class NativeMethods {
        #region Window
        /// <summary>
        /// Copies the text of the specified window's title bar (if it has one) into a buffer.
        /// If the specified window is a control, the text of the control is copied.
        /// However, GetWindowText cannot retrieve the text of a control in another application.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control containing the text.</param>
        /// <param name="lpString">The buffer that will receive the text. If the string is as long or longer than the buffer, the string is truncated and terminated with a null character.</param>
        /// <param name="nMaxCount">The maximum number of characters to copy to the buffer, including the null character. If the text exceeds this limit, it is truncated.</param>
        /// <returns>If the function succeeds, the return value is the length, in characters, of the copied string, not including the terminating null character. If the window has no title bar or text, if the title bar is empty, or if the window or control handle is invalid, the return value is zero.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// Retrieves the length, in characters, of the specified window's title bar text (if the window has a title bar).
        /// If the specified window is a control, the function retrieves the length of the text within the control.
        /// However, GetWindowTextLength cannot retrieve the length of the text of an edit control in another application.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control.</param>
        /// <returns>If the function succeeds, the return value is the length, in characters, of the text. Under certain conditions, this value might be greater than the length of the text (see Remarks). If the window has no text, the return value is zero. Function failure is indicated by a return value of zero and a GetLastError result that is nonzero.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        #endregion

        #region WndProc
        // Messages https://wiki.winehq.org/List_Of_Windows_Messages
        public enum WindowProcMessage : int {
            /// <summary>
            /// A window receives this message when the user chooses a command from the Window menu (formerly known as the system or control menu)
            /// or when the user chooses the maximize button, minimize button, restore button, or close button.
            /// </summary>
            /// <see cref="WindowMessageSystemCommand"/>
            /// <see href="https://docs.microsoft.com/en-us/windows/win32/menurc/wm-syscommand"/>
            WM_SYSCOMMAND = 0x0112
        }

        /// <summary>
        /// The type of system command requested.
        /// </summary>
        /// <see cref="WindowProcMessage.WM_SYSCOMMAND"/>
        public enum WindowMessageSystemCommand : int {
            /// <summary>
            /// Closes the window.
            /// </summary>
            SC_CLOSE = 0xF060,

            /// <summary>
            /// Changes the cursor to a question mark with a pointer. If the user then clicks a control in the dialog box, the control receives a WM_HELP message.
            /// </summary>
            SC_CONTEXTHELP = 0xF180,

            /// <summary>
            /// Selects the default item; the user double-clicked the window menu.
            /// </summary>
            SC_DEFAULT = 0xF160,

            /// <summary>
            /// Activates the window associated with the application-specified hot key. The lParam parameter identifies the window to activate.
            /// </summary>
            SC_HOTKEY = 0xF150,

            /// <summary>
            /// Scrolls horizontally.
            /// </summary>
            SC_HSCROLL = 0xF080,

            /// <summary>
            /// Indicates whether the screen saver is secure.
            /// </summary>
            SCF_ISSECURE = 0x00000001,

            /// <summary>
            /// Retrieves the window menu as a result of a keystroke. For more information, see the Remarks section.
            /// </summary>
            SC_KEYMENU = 0xF100,

            /// <summary>
            /// Maximizes the window.
            /// </summary>
            SC_MAXIMIZE = 0xF030,

            /// <summary>
            /// Minimizes the window.
            /// </summary>
            SC_MINIMIZE = 0xF020,

            /// <summary>
            /// Sets the state of the display. This command supports devices that have power-saving features, such as a battery-powered personal computer.
            /// </summary>
            SC_MONITORPOWER = 0xF170,

            /// <summary>
            /// Retrieves the window menu as a result of a mouse click.
            /// </summary>
            SC_MOUSEMENU = 0xF090,

            /// <summary>
            /// Moves the window.
            /// </summary>
            SC_MOVE = 0xF010,

            /// <summary>
            /// Moves to the next window.
            /// </summary>
            SC_NEXTWINDOW = 0xF040,

            /// <summary>
            /// Moves to the previous window.
            /// </summary>
            SC_PREVWINDOW = 0xF050,

            /// <summary>
            /// Restores the window to its normal position and size.
            /// </summary>
            SC_RESTORE = 0xF120,

            /// <summary>
            /// Executes the screen saver application specified in the [boot] section of the System.ini file.
            /// </summary>
            SC_SCREENSAVE = 0xF140,

            /// <summary>
            /// Sizes the window.
            /// </summary>
            SC_SIZE = 0xF000,

            /// <summary>
            /// Activates the Start menu.
            /// </summary>
            SC_TASKLIST = 0xF130,

            /// <summary>
            /// Scrolls vertically.
            /// </summary>
            SC_VSCROLL = 0xF070

        }
        #endregion

        #region Process
        /// <summary>
        /// Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of the process that created the window.
        /// </summary>
        /// <param name="hWnd">A handle to the window.</param>
        /// <param name="lpdwProcessId">A pointer to a variable that receives the process identifier. If this parameter is not NULL, GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it does not.</param>
        /// <returns>The return value is the identifier of the thread that created the window.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        #endregion

        #region FlashWindow
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
            /// This is equivalent to setting the <see cref="FLASHW_CAPTION" /> | <see cref="FLASHW_TRAY" /> flags.
            /// </summary>
            FLASHW_ALL = FLASHW_CAPTION | FLASHW_TRAY,
            /// <summary>
            /// Flash continuously, until the <see cref="FLASHW_STOP" /> flag is set.
            /// </summary>
            FLASHW_TIMER = 4,
            /// <summary>
            /// Flash continuously until the window comes to the foreground.
            /// </summary>
            FLASHW_TIMERNOFG = 12
        }
        #endregion

    }
}
