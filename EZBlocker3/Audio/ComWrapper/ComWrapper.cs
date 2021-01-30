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
                    // dispose managed state
                }

                // free unmanaged resources
                if (ComObject != null)
                    Marshal.ReleaseComObject(ComObject);

                _disposed = true;
            }
        }

        ~ComWrapper() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
