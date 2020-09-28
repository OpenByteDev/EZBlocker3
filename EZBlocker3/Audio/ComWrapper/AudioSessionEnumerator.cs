#nullable disable

using EZBlocker3.Audio.Com;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.ComWrapper {
    internal class AudioSessionEnumerator : ComWrapper<IAudioSessionEnumerator>, IEnumerator<AudioSession> {

        private int _index = 0;
        private int _count;

        public AudioSessionEnumerator(IAudioSessionEnumerator sessionEnumerator) : base(sessionEnumerator) {
            Marshal.ThrowExceptionForHR(sessionEnumerator.GetCount(out _count));
        }

        public AudioSession Current { get; private set; }
        object IEnumerator.Current => Current;

        public bool MoveNext() {
            if (_index >= _count)
                return false;

            Marshal.ThrowExceptionForHR(ComObject.GetSession(_index, out var session));
            Current = new AudioSession(session);
            _index++;
            return true;
        }

        public void Reset() {
            throw new NotImplementedException();
        }
    }
}
