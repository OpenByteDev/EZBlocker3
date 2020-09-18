using EZBlocker3.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows.Forms;
using static EZBlocker3.AudioUtils;
using Timer = System.Timers.Timer;

namespace EZBlocker3 {
    internal class SpotifyHook : IDisposable {

        public Process? Process { get; private set; }
        private VolumeControl? volumeControl;
        public VolumeControl? VolumeControl {
            get {
                volumeControl ??= AudioUtils.GetVolumeControl(allSpotifyProcessIds);
                return volumeControl;
            }
            private set => volumeControl = value;
        }

        public string? WindowName { get; private set; }
        public bool IsHooked => Process != null && !Process.HasExited;
        public bool IsPaused { get; private set; }
        public bool IsPlaying => !IsPaused;
        public bool IsSongPlaying => ActiveSong != null;
        public bool IsAdPlaying { get; private set; }
        public bool? IsMuted { get; private set; }
        public SongInfo? ActiveSong { get; private set; }
        public bool IsActive { get; private set; }

        public event ActiveSongChangedEventHandler? ActiveSongChanged;
        public delegate void ActiveSongChangedEventHandler(object sender, EventArgs eventArgs);
        public event SpotifyStateChangedEventHandler? SpotifyStateChanged;
        public delegate void SpotifyStateChangedEventHandler(object sender, EventArgs eventArgs);

        private readonly Timer RefreshTimer = new Timer(100);
        private HashSet<int>? allSpotifyProcessIds;

        // private float peak = 0f;
        // private float lastPeak = 0f;

        public SpotifyHook() {
            RefreshTimer.Elapsed += RefreshTimer_Elapsed;
        }

        public void Activate() {
            IsActive = true;

            HookSpotify();

            RefreshTimer.Start();
        }
        public void Deactivate() {
            IsActive = false;

            RefreshTimer.Stop();

            ClearHook();
        }

        private void RefreshTimer_Elapsed(object sender, ElapsedEventArgs e) {
            lock (this) {
                if (!IsHooked) {
                    HookSpotify();
                } else {
                    RefreshHook();
                }
            }
        }

        private bool HookSpotify() {
            Process[] processes = Process.GetProcessesByName("spotify");

            Process = processes.Where(e => !string.IsNullOrWhiteSpace(e.MainWindowTitle)).FirstOrDefault();
            if (Process is null) {
                if (VolumeControl is null)
                    return false;

                try {
                    Process = Process.GetProcessById(VolumeControl.ProcessId);
                } catch(ArgumentException) {
                    // TODO rework VolumeControl to store Process so that HasExited can be used.
                    Process = null;
                }
                if (!IsHooked) {
                    Process = null;
                    VolumeControl = null;
                    return false;
                }
            }

            allSpotifyProcessIds = processes.Select(e => e.Id).ToHashSet();
            UpdateInfo();
            OnSpotifyStateChanged();

            return true;
        }

        private void ClearHook() {
            VolumeControl?.Dispose();
            Process?.Dispose();

            Process = null;
            VolumeControl = null;
        }

        private void RefreshHook() {
            if (Process is null)
                return;

            // Process.Refresh();
            var id = Process.Id;
            Process.Dispose();
            Process = Process.GetProcessById(id);
            UpdateInfo();
        }

        private void UpdateInfo() {
            UpdateMuteStatus();

            string? oldWindowName = WindowName;
            string? newWindowName = WindowName = Process?.MainWindowTitle.Trim();
            if (oldWindowName != newWindowName) {
                var prevState = (ActiveSong, IsAdPlaying, IsSongPlaying);

                var volume = AudioUtils.GetPeakVolume(VolumeControl?.Control);
                if (string.IsNullOrWhiteSpace(newWindowName) || (newWindowName.StartsWith("Spotify") && volume == 0)) {
                    ActiveSong = null;
                    IsPaused = true;
                    IsAdPlaying = false;
                } else {
                    IsPaused = false;
                    if (newWindowName.Contains('-')) {
                        (string artist, string title) = newWindowName.Split('-').Select(e => e.Trim()).ToArray();
                        ActiveSong = new SongInfo(title, artist);
                        IsAdPlaying = false;
                    } else /* if (newWindowName == "Advertisement") */ {
                        ActiveSong = null;
                        IsAdPlaying = true;
                    }
                }

                if (prevState != (ActiveSong, IsAdPlaying, IsSongPlaying))
                    OnSpotifyStateChanged();
                if (prevState.ActiveSong != ActiveSong)
                    OnActiveSongChanged();
            }
        }

        private void UpdateMuteStatus() {
            IsMuted ??= AudioUtils.IsMuted(VolumeControl?.Control);
        }

        public void Mute() => SetMute(mute: true);
        public void Unmute() => SetMute(mute: false);
        public void SetMute(bool mute) {
            if (IsMuted == mute) {
                return;
            }

            if (VolumeControl != null) {
                AudioUtils.SetMute(VolumeControl.Control, mute);
                IsMuted = mute;
            } else {
                IsMuted = null;
            }
        }

        private void OnActiveSongChanged() => OnActiveSongChanged(EventArgs.Empty);
        protected virtual void OnActiveSongChanged(EventArgs eventArgs) {
            ActiveSongChanged?.Invoke(this, eventArgs);
        }

        private void OnSpotifyStateChanged() => OnSpotifyStateChanged(EventArgs.Empty);
        protected virtual void OnSpotifyStateChanged(EventArgs eventArgs) {
            SpotifyStateChanged?.Invoke(this, eventArgs);
        }

        private bool _disposed;
        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    // dispose managed state
                    RefreshTimer?.Dispose();
                    VolumeControl?.Dispose();
                    Process?.Dispose();
                }

                // free unmanaged resources

                _disposed = true;
            }
        }

        public void Dispose() {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
