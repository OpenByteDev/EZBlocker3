using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Audio.CoreAudio {
    public unsafe class AudioSessionCollection : CriticalFinalizerObject, IDisposable {
        private readonly IAudioSessionEnumerator sessionEnumerator;

        public AudioSessionCollection(IAudioSessionEnumerator sessionEnumerator) {
            this.sessionEnumerator = sessionEnumerator;
        }

        public AudioSession this[int index] {
            get {
                sessionEnumerator.GetSession(index, out var session);
                return new AudioSession(session);
            }
        }

        public int Count {
            get {
                sessionEnumerator.GetCount(out var count);
                return count;
            }
        }

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                _disposed = true;

                if (sessionEnumerator != null)
                    Marshal.FinalReleaseComObject(sessionEnumerator);
            }
        }

        ~AudioSessionCollection() {
            Dispose(disposing: false);
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
