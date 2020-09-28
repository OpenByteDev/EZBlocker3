using EZBlocker3.Audio.Com;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.ComWrapper {
    internal class AudioSessionManager : ComWrapper<IAudioSessionManager2> {

        public AudioSessionManager(IAudioSessionManager2 sessionManager) : base(sessionManager) { }

        public IEnumerable<AudioSession> GetSessions() {
            return new Enumerable(this);
        }

        public AudioSessionCollection GetSessionCollection() {
            IAudioSessionEnumerator? sessionEnumerator = null;
            try {
                Marshal.ThrowExceptionForHR(ComObject.GetSessionEnumerator(out sessionEnumerator));
                return new AudioSessionCollection(sessionEnumerator);
            } catch {
                if (sessionEnumerator != null)
                    Marshal.ReleaseComObject(sessionEnumerator);
                throw;
            }
        }

        class Enumerable : IEnumerable<AudioSession> {
            private AudioSessionManager _audioSessionManager;

            public Enumerable(AudioSessionManager audioSessionManager) {
                _audioSessionManager = audioSessionManager;
            }

            public IEnumerator<AudioSession> GetEnumerator() {
                IAudioSessionEnumerator? sessionEnumerator = null;
                try {
                    Marshal.ThrowExceptionForHR(_audioSessionManager.ComObject.GetSessionEnumerator(out sessionEnumerator));
                    if (sessionEnumerator == null)
                        return System.Linq.Enumerable.Empty<AudioSession>().GetEnumerator();
                    return new AudioSessionEnumerator(sessionEnumerator);
                } finally {
                    if (sessionEnumerator != null)
                        Marshal.ReleaseComObject(sessionEnumerator);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

    }
}