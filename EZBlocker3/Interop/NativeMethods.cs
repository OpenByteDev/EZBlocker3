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
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// Retrieves the length, in characters, of the specified window's title bar text (if the window has a title bar).
        /// If the specified window is a control, the function retrieves the length of the text within the control.
        /// However, GetWindowTextLength cannot retrieve the length of the text of an edit control in another application.
        /// </summary>
        /// <param name="hWnd">A handle to the window or control.</param>
        /// <returns>If the function succeeds, the return value is the length, in characters, of the text. Under certain conditions, this value might be greater than the length of the text (see Remarks). If the window has no text, the return value is zero. Function failure is indicated by a return value of zero and a GetLastError result that is nonzero.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
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

        #region WinEvent
        /// <summary>
        /// An application-defined callback (or hook) function that the system calls in response to events generated by an accessible object.
        /// The hook function processes the event notifications as required.
        /// Clients install the hook function and request specific types of event notifications by calling SetWinEventHook.
        /// </summary>
        /// <param name="hWinEventHook">Handle to an event hook function. This value is returned by SetWinEventHook when the hook function is installed and is specific to each instance of the hook function.</param>
        /// <param name="eventType">Specifies the event that occurred. </param>
        /// <param name="hwnd">Handle to the window that generates the event, or NULL if no window is associated with the event. For example, the mouse pointer is not associated with a window.</param>
        /// <param name="idObject">Identifies the object associated with the event. This is one of the object identifiers or a custom object ID.</param>
        /// <param name="idChild">Identifies whether the event was triggered by an object or a child element of the object. If this value is CHILDID_SELF, the event was triggered by the object; otherwise, this value is the child ID of the element that triggered the event.</param>
        /// <param name="dwEventThread"></param>
        /// <param name="dwmsEventTime">Specifies the time, in milliseconds, that the event was generated.</param>
        public delegate void WinEventProc(
            IntPtr hWinEventHook,
            WindowEvent eventType,
            IntPtr hwnd,
            AccessibleObjectID idObject,
            int idChild,
            uint dwEventThread,
            uint dwmsEventTime);

        public const int CHILDID_SELF = 0;

        /// <summary>
        /// This enumeration lists all kinds of accessible objects that can
        /// be directly assigned to a window.
        /// </summary>
        public enum AccessibleObjectID : uint {
            /// <summary>
            /// The window itself rather than a child object.
            /// </summary>
            OBJID_WINDOW = 0x00000000,

            /// <summary>
            /// The window's system menu.
            /// </summary>
            OBJID_SYSMENU = 0xFFFFFFFF,

            /// <summary>
            /// The window's title bar.
            /// </summary>
            OBJID_TITLEBAR = 0xFFFFFFFE,

            /// <summary>
            /// The window's menu bar.
            /// </summary>
            OBJID_MENU = 0xFFFFFFFD,

            /// <summary>
            /// The window's client area. 
            /// </summary>
            OBJID_CLIENT = 0xFFFFFFFC,

            /// <summary>
            /// The window's vertical scroll bar.
            /// </summary>
            OBJID_VSCROLL = 0xFFFFFFFB,

            /// <summary>
            /// The window's horizontal scroll bar.
            /// </summary>
            OBJID_HSCROLL = 0xFFFFFFFA,

            /// <summary>
            /// The window's size grip: an optional frame component located at the lower-right corner of the window frame.
            /// </summary>
            OBJID_SIZEGRIP = 0xFFFFFFF9,

            /// <summary>
            /// The text insertion bar (caret) in the window.
            /// </summary>
            OBJID_CARET = 0xFFFFFFF8,

            /// <summary>
            /// The mouse pointer. There is only one mouse pointer in the system, and it is not a child of any window.
            /// </summary>
            OBJID_CURSOR = 0xFFFFFFF7,

            /// <summary>
            /// An alert that is associated with a window or an application. 
            /// </summary>
            OBJID_ALERT = 0xFFFFFFF6,

            /// <summary>
            /// A sound object. Sound objects do not have screen locations or children, but they do have name and state attributes. They are children of the application that is playing the sound.
            /// </summary>
            OBJID_SOUND = 0xFFFFFFF5
        }

        [Flags]
        public enum WinEventHookFlags : uint {
            /// <summary>
            /// The callback function is not mapped into the address space of the process that generates the event.
            /// Because the hook function is called across process boundaries, the system must queue events.
            /// Although this method is asynchronous, events are guaranteed to be in sequential order.
            /// </summary>
            WINEVENT_OUTOFCONTEXT = 0x0000,

            /// <summary>
            /// Prevents this instance of the hook from receiving the events that are generated by the thread that is registering this hook. 
            /// </summary>
            WINEVENT_SKIPOWNTHREAD = 0x0001,

            /// <summary>
            /// Prevents this instance of the hook from receiving the events that are generated by threads in this process.
            /// This flag does not prevent threads from generating events.
            /// </summary>
            WINEVENT_SKIPOWNPROCESS = 0x0002,

            /// <summary>
            /// The DLL that contains the callback function is mapped into the address space of the process that generates the event.
            /// With this flag, the system sends event notifications to the callback function as they occur. 
            /// The hook function must be in a DLL when this flag is specified.
            /// This flag has no effect when both the calling process and the generating process are not 32-bit or 64-bit processes, or when the generating process is a console application.
            /// </summary>
            WINEVENT_INCONTEXT = 0x0004
        }

        public enum WindowEvent : uint {
            /// <summary>
            /// The lowest possible event value.
            /// </summary>
            EVENT_MIN = 0x00000001,
            /// <summary>
            /// The highest possible event value.
            /// </summary>
            EVENT_MAX = 0x7FFFFFFF,
            /// <summary>
            /// An object has been created.
            /// The system sends this event for the following user interface elements: 
            /// caret, header control, list-view control, tab control, toolbar control, tree view control, and window object.
            /// </summary>
            EVENT_OBJECT_CREATE = 0x8000,
            /// <summary>
            /// n object has been destroyed.
            /// The system sends this event for the following user interface elements:
            /// caret, header control, list-view control, tab control, toolbar control, tree view control, and window object.
            /// </summary>
            EVENT_OBJECT_DESTROY = 0x8001,
            /// <summary>
            /// An object's Name property has changed.
            /// The system sends this event for the following user interface elements: check box, cursor, list-view control, push button, radio button, status bar control, tree view control, and window object.
            /// Server applications send this event for their accessible objects.
            /// </summary>
            EVENT_OBJECT_NAMECHANGE = 0x800C,
            /// <summary>
            /// A sound has been played.
            /// The system sends this event when a system sound, such as one for a menu, is played even if no sound is audible (for example, due to the lack of a sound file or a sound card).
            /// Servers send this event whenever a custom UI element generates a sound. 
            /// </summary>
            EVENT_SYSTEM_SOUND = 0x0001,
        }

        /// <summary>
        /// Sets an event hook function for a range of events.
        /// </summary>
        /// <param name="eventMin">Specifies the event constant for the lowest event value in the range of events that are handled by the hook function. This parameter can be set to EVENT_MIN to indicate the lowest possible event value.</param>
        /// <param name="eventMax">Specifies the event constant for the highest event value in the range of events that are handled by the hook function. This parameter can be set to EVENT_MAX to indicate the highest possible event value.</param>
        /// <param name="hmodWinEventProc">Handle to the DLL that contains the hook function at lpfnWinEventProc, if the WINEVENT_INCONTEXT flag is specified in the dwFlags parameter. If the hook function is not located in a DLL, or if the WINEVENT_OUTOFCONTEXT flag is specified, this parameter is NUL</param>
        /// <param name="lpfnWinEventProc">Pointer to the event hook function.</param>
        /// <param name="idProcess">Specifies the ID of the process from which the hook function receives events. Specify zero (0) to receive events from all processes on the current desktop.</param>
        /// <param name="idThread">Specifies the ID of the thread from which the hook function receives events. If this parameter is zero, the hook function is associated with all existing threads on the current desktop.</param>
        /// <param name="dwFlags">Flag values that specify the location of the hook function and of the events to be skipped.</param>
        /// <returns>If successful, returns an HWINEVENTHOOK value that identifies this event hook instance. Applications save this return value to use it with the UnhookWinEvent function. If unsuccessful, returns zero.</returns>
        /// <see href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwineventhook"/>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWinEventHook(WindowEvent eventMin, WindowEvent eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc,
                                                    uint idProcess, uint idThread, WinEventHookFlags dwFlags);

        /// <summary>
        /// Removes an event hook function created by a previous call to SetWinEventHook.
        /// </summary>
        /// <param name="hWinEventHook">Handle to the event hook returned in the previous call to SetWinEventHook.</param>
        /// <returns>If successful, returns TRUE; otherwise, returns FALSE.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);
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
            FLASHW_ALL = 3,
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
