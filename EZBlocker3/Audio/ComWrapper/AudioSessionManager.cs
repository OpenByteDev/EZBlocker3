using EZBlocker3.Audio.Com;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.ComWrapper {
    internal class AudioSessionManager : ComWrapper<IAudioSessionManager2> {

        public AudioSessionManager(IAudioSessionManager2 sessionManager) : base(sessionManager) { }

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

    }
}