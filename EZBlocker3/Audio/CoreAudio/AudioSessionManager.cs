using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Audio.CoreAudio {
    public unsafe class AudioSessionManager : CriticalFinalizerObject, IDisposable {
        private readonly IAudioSessionManager2* sessionManager;

        public AudioSessionManager(IAudioSessionManager2* sessionManager) {
            this.sessionManager = sessionManager;
        }

        public AudioSessionCollection GetSessionCollection() {
            IAudioSessionEnumerator* sessionEnumerator = null;
            try {
                Marshal.ThrowExceptionForHR(sessionManager->GetSessionEnumerator(ref sessionEnumerator));
                return new AudioSessionCollection(sessionEnumerator);
            } catch {
                if (sessionEnumerator != null)
                    sessionEnumerator->Release();
                throw;
            }
        }

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                _disposed = true;

                if (sessionManager != null)
                    sessionManager->Release();
            }
        }

        ~AudioSessionManager() {
            Dispose(disposing: false);
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}