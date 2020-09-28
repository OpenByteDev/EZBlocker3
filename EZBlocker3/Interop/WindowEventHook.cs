#nullable disable

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using static EZBlocker3.Interop.NativeMethods;

namespace EZBlocker3.Interop {
    internal class WindowEventHook : IDisposable {

        public WindowEvent EventMin { get; private set; }
        public WindowEvent EventMax { get; private set; }
        public bool Hooked { get; private set; } = false;

        public event WinEventProc WinEventProc;

        private GCHandle _eventProcHandle;
        private IntPtr _hookHandle;

        public WindowEventHook() : this(WindowEvent.EVENT_MIN, WindowEvent.EVENT_MAX) { }
        public WindowEventHook(WindowEvent @event) : this(@event, @event) { }
        public WindowEventHook(WindowEvent eventMin, WindowEvent eventMax) {
            EventMin = eventMin;
            EventMax = eventMax;
        }

        public bool HookGlobal() => HookInternal(0, 0);
        public bool HookToProcess(Process process) => HookToProcess((uint) process.Id);
        public bool HookToProcess(uint processId) => HookInternal(processId, 0);
        public bool HookToThread(Thread thread) => HookToThread((uint) thread.ManagedThreadId);
        public bool HookToThread(uint threadId) => HookInternal(0, threadId);

        private bool HookInternal(uint processId, uint threadId) {
            if (Hooked)
                throw new InvalidOperationException("Hook is already hooked.");

            var eventProc = new WinEventProc(OnWinEventProc);
            _eventProcHandle = GCHandle.Alloc(eventProc);
            _hookHandle = SetWinEventHook(
                eventMin: EventMin,
                eventMax: EventMax,
                hmodWinEventProc: IntPtr.Zero,
                lpfnWinEventProc: eventProc, 
                idProcess: processId, 
                idThread: threadId, 
                dwFlags: WinEventHookFlags.WINEVENT_OUTOFCONTEXT | WinEventHookFlags.WINEVENT_SKIPOWNPROCESS
            );

            Hooked = _hookHandle != IntPtr.Zero;

            if (!Hooked)
                throw new Win32Exception();
            return Hooked;
        }

        public void Unhook() {
            if (!Hooked)
                throw new InvalidOperationException("Hook is not hooked.");

            UnhookInternal();
        }

        private void UnhookInternal() {
            Hooked = false;

            if (_hookHandle != IntPtr.Zero)
                UnhookWinEvent(_hookHandle);
            if (_eventProcHandle.IsAllocated)
                _eventProcHandle.Free();
        }

        protected virtual void OnWinEventProc(IntPtr hWinEventHook, WindowEvent eventType, IntPtr hwnd, AccessibleObjectID idObject, int idChild, uint dwEventThread, uint dwmsEventTime) {
            WinEventProc?.Invoke(hWinEventHook, eventType, hwnd, idObject, idChild, dwEventThread, dwEventThread);
        }

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // dispose managed state

                }

                // free unmanaged resources
                UnhookInternal();

                _disposed = true;
            }
        }

        ~WindowEventHook() {
             // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
             Dispose(disposing: false);
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
