using System.Runtime.InteropServices;
using EZBlocker3.Audio.Com;

namespace EZBlocker3.Audio.ComWrapper {
    public class AudioSessionManager : ComWrapper<IAudioSessionManager2> {
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