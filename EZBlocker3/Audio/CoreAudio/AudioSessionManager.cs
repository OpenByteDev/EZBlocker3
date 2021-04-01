using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Audio.CoreAudio {
    public unsafe class AudioSessionManager : CriticalFinalizerObject, IDisposable {
        private readonly IAudioSessionManager2 sessionManager;

        public AudioSessionManager(IAudioSessionManager2 sessionManager) {
            this.sessionManager = sessionManager;
        }

        public AudioSessionCollection GetSessionCollection() {
            IAudioSessionEnumerator? sessionEnumerator = null;
            try {
                sessionEnumerator = sessionManager.GetSessionEnumerator();
                return new AudioSessionCollection(sessionEnumerator);
            } catch {
                if (sessionEnumerator != null)
                    Marshal.FinalReleaseComObject(sessionEnumerator);
                throw;
            }
        }

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                _disposed = true;

                if (sessionManager != null)
                    Marshal.FinalReleaseComObject(sessionManager);
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