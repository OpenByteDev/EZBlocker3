using System;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Audio.CoreAudio {
    public unsafe class AudioDevice : IDisposable {
        private readonly IMMDevice device;

        public AudioDevice(IMMDevice device) {
            this.device = device;
        }

        public static AudioDevice GetDefaultAudioDevice(EDataFlow dataFlow, ERole role) {
            IMMDeviceEnumerator? deviceEnumerator = null;
            try {
                PInvoke.CoCreateInstance(typeof(MMDeviceEnumerator).GUID, null, (uint)CLSCTX.CLSCTX_INPROC_SERVER, typeof(IMMDeviceEnumerator).GUID, out var tmp);
                deviceEnumerator = (IMMDeviceEnumerator)tmp;
                deviceEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out IMMDevice device);
                return new AudioDevice(device);
            } finally {
                if (deviceEnumerator != null)
                    Marshal.FinalReleaseComObject(deviceEnumerator);
            }
        }

        public AudioSessionManager GetSessionManager() {
            device.Activate(typeof(IAudioSessionManager2).GUID, 0, default, out var sessionManager);
            return new AudioSessionManager((IAudioSessionManager2)Marshal.GetObjectForIUnknown((IntPtr)sessionManager));
        }

        #region IDisposable
        private bool isDisposed;
        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                isDisposed = true;

                Marshal.FinalReleaseComObject(device);
            }
        }

        ~AudioDevice() {
            Dispose(disposing: false);
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable
    }
}
