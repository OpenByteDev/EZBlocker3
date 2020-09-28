using EZBlocker3.Audio.Com;
using System;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.ComWrapper {
    internal class AudioDevice : ComWrapper<IMMDevice> {

        private AudioDevice(IMMDevice device) : base(device) { }

        public static AudioDevice GetDefaultAudioDevice(EDataFlow dataFlow, ERole role) {
            IMMDeviceEnumerator? deviceEnumerator = null;
            try {
                deviceEnumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
                Marshal.ThrowExceptionForHR(deviceEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out var device));
                return new AudioDevice(device);
            } finally {
                if (deviceEnumerator != null)
                    Marshal.ReleaseComObject(deviceEnumerator);
            }
        }

        public AudioSessionManager GetSessionManager() {
            Marshal.ThrowExceptionForHR(ComObject.Activate(typeof(IAudioSessionManager2).GUID, 0, IntPtr.Zero, out var sessionManager));
            return new AudioSessionManager((IAudioSessionManager2)sessionManager);
        }

    }
}
