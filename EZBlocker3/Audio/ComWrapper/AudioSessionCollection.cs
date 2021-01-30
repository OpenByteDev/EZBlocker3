using EZBlocker3.Audio.Com;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.ComWrapper {
    public class AudioSessionCollection : ComWrapper<IAudioSessionEnumerator> {
        public AudioSessionCollection(IAudioSessionEnumerator sessionEnumerator) : base(sessionEnumerator) { }

        public AudioSession this[int index] {
            get {
                Marshal.ThrowExceptionForHR(ComObject.GetSession(index, out var result));
                return new AudioSession(result);
            }
        }

        public int Count {
            get {
                Marshal.ThrowExceptionForHR(ComObject.GetCount(out var result));
                return result;
            }
        }
    }
}
