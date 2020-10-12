using EZBlocker3.Audio.Com;
using System;
using System.Runtime.InteropServices;

namespace EZBlocker3.Audio.ComWrapper {
    public class AudioSession : IDisposable {

        private readonly IAudioSessionControl _audioSessionControl;
        private readonly IAudioSessionControl2? _audioSessionControl2;
        private readonly ISimpleAudioVolume? _simpleAudioVolume;
        private readonly IAudioMeterInformation? _audioMeterInformation;

        public AudioSession(IAudioSessionControl session) {
            _audioSessionControl = session;
            _simpleAudioVolume = session as ISimpleAudioVolume;
            _audioMeterInformation = session as IAudioMeterInformation;
            _audioSessionControl2 = session as IAudioSessionControl2;
        }

        public uint ProcessID {
            get {
                if (_audioSessionControl2 is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(_audioSessionControl2.GetProcessId(out var processId));
                return processId;
            }
        }

        public bool IsMuted {
            get {
                if (_simpleAudioVolume is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(_simpleAudioVolume.GetMute(out var isMuted));
                return isMuted;
            }
            set {
                if (_simpleAudioVolume is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(_simpleAudioVolume.SetMute(value, Guid.Empty));
            }
        }

        public float MasterVolume {
            get {
                if (_simpleAudioVolume is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(_simpleAudioVolume.GetMasterVolume(out var level));
                return level;
            }
            set {
                if (_simpleAudioVolume is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(_simpleAudioVolume.SetMasterVolume(value, Guid.Empty));
            }
        }

        public float PeakVolume {
            get {
                if (_audioMeterInformation is null)
                    throw new NotSupportedException();
                Marshal.ThrowExceptionForHR(_audioMeterInformation.GetPeakValue(out var peak));
                return peak;
            }
        }

        #region IDisposable
        private bool _disposed;
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // dispose managed state
                }

                // free unmanaged resources
                if (_audioSessionControl != null)
                    Marshal.ReleaseComObject(_audioSessionControl);
                // if (_audioSessionControl2 != null)
                //     Marshal.ReleaseComObject(_audioSessionControl2);
                // if (_simpleAudioVolume != null)
                //     Marshal.ReleaseComObject(_simpleAudioVolume);
                // if (_audioMeterInformation != null)
                //     Marshal.ReleaseComObject(_audioMeterInformation);

                _disposed = true;
            }
        }

        ~AudioSession() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}