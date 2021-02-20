using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using Microsoft.Windows.Sdk;

namespace EZBlocker3.Audio.CoreAudio {
    public unsafe class AudioSession : CriticalFinalizerObject, IDisposable {
        private readonly IAudioSessionControl* audioSessionControl;
        private readonly IAudioSessionControl2* audioSessionControl2;
        private readonly ISimpleAudioVolume* simpleAudioVolume;
        private readonly IAudioMeterInformation* audioMeterInformation;

        public AudioSession(IAudioSessionControl* session) {
            audioSessionControl = session;

            session->QueryInterface(typeof(ISimpleAudioVolume).GUID, out var sav);
            simpleAudioVolume = (ISimpleAudioVolume*)sav;

            session->QueryInterface(typeof(IAudioMeterInformation).GUID, out var ami);
            audioMeterInformation = (IAudioMeterInformation*)ami;

            session->QueryInterface(typeof(IAudioSessionControl2).GUID, out var asc2);
            audioSessionControl2 = (IAudioSessionControl2*)asc2;
        }

        public uint ProcessID {
            get {
                if (audioSessionControl2 is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(audioSessionControl2->GetProcessId(out var processId));
                return processId;
            }
        }

        public bool IsMuted {
            get {
                if (simpleAudioVolume is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(simpleAudioVolume->GetMute(out var isMuted));
                return isMuted;
            }
            set {
                if (simpleAudioVolume is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(simpleAudioVolume->SetMute(value, default));
            }
        }

        public float MasterVolume {
            get {
                if (simpleAudioVolume is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(simpleAudioVolume->GetMasterVolume(out var level));
                return level;
            }
            set {
                if (simpleAudioVolume is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(simpleAudioVolume->SetMasterVolume(value, default));
            }
        }

        public float PeakVolume {
            get {
                if (audioMeterInformation is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(audioMeterInformation->GetPeakValue(out var peak));
                return peak;
            }
        }

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                _disposed = true;

                if (audioSessionControl != null)
                    audioSessionControl->Release();
                if (audioSessionControl2 != null)
                    audioSessionControl2->Release();
                if (simpleAudioVolume != null)
                    simpleAudioVolume->Release();
                if (audioMeterInformation != null)
                    audioMeterInformation->Release();
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