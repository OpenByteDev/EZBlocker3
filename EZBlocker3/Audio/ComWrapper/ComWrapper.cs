using System;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio {
    public abstract class ComWrapper<T> : IDisposable {
        protected readonly T ComObject;

        protected ComWrapper(T obj) {
            ComObject = obj;
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // no managed objects to dispose
                }

                // free unmanaged resources
                if (ComObject != null)
                    Marshal.ReleaseComObject(ComObject);

                _disposed = true;
            }
        }

        ~ComWrapper() {
            Dispose(disposing: false);
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
