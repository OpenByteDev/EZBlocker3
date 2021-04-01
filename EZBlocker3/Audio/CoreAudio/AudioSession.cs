using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Audio.CoreAudio {
    public unsafe class AudioSession : CriticalFinalizerObject, IDisposable {
        private readonly IAudioSessionControl audioSessionControl;
        private readonly IAudioSessionControl2? audioSessionControl2;
        private readonly ISimpleAudioVolume? simpleAudioVolume;
        private readonly IAudioMeterInformation? audioMeterInformation;

        public AudioSession(IAudioSessionControl session) {
            audioSessionControl = session;

            simpleAudioVolume = session as ISimpleAudioVolume;
            audioMeterInformation = session as IAudioMeterInformation;
            audioSessionControl2 = session as IAudioSessionControl2;
        }

        public uint ProcessID {
            get {
                if (audioSessionControl2 is null)
                    throw new NotSupportedException();
                audioSessionControl2.GetProcessId(out var processId);
                return processId;
            }
        }

        public bool IsMuted {
            get {
                if (simpleAudioVolume is null)
                    throw new NotSupportedException();
                simpleAudioVolume.GetMute(out var isMuted);
                return isMuted;
            }
            set {
                if (simpleAudioVolume is null)
                    throw new NotSupportedException();
                simpleAudioVolume.SetMute(value, default);
            }
        }

        public float MasterVolume {
            get {
                if (simpleAudioVolume is null)
                    throw new NotSupportedException();
                simpleAudioVolume.GetMasterVolume(out var level);
                return level;
            }
            set {
                if (simpleAudioVolume is null)
                    throw new NotSupportedException();
                simpleAudioVolume.SetMasterVolume(value, default);
            }
        }

        public float PeakVolume {
            get {
                if (audioMeterInformation is null)
                    throw new NotSupportedException();
                audioMeterInformation.GetPeakValue(out var peak);
                return peak;
            }
        }

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                _disposed = true;

                if (audioSessionControl != null)
                    Marshal.FinalReleaseComObject(audioSessionControl);
                if (audioSessionControl2 != null)
                    Marshal.FinalReleaseComObject(audioSessionControl2);
                if (simpleAudioVolume != null)
                    Marshal.FinalReleaseComObject(simpleAudioVolume);
                if (audioMeterInformation != null)
                    Marshal.FinalReleaseComObject(audioMeterInformation);
            }
        }

        ~AudioSession() {
            Dispose(disposing: false);
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}