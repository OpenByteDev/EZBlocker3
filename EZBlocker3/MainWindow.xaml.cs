using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace EZBlocker3 {
    public partial class MainWindow : Window {

        private readonly SpotifyHook spotifyHook = new SpotifyHook();
        private readonly NotifyIcon _notifyIcon = new NotifyIcon();

        public MainWindow() {
            InitializeComponent();

            OpenVolumeControlButton.Click += OpenVolumeControlButton_Click;
            // MuteSpotifyButton.Click += MuteSpotifyButton_Click;

            SetupSpotifyHook();
            SetupNotifyIcon();

            UpdateStatusLabel();
            UpdateMuteStatus();
        }

        private void SetupSpotifyHook() {
            spotifyHook.SpotifyStateChanged += SpotifyHook_SpotifyStateChanged;
            spotifyHook.Activate();
        }

        private void SetupNotifyIcon() {
            var sri = Application.GetResourceStream(new Uri("/Icon/Icon32.ico", UriKind.Relative));
            if (sri != null)
                _notifyIcon.Icon = new Icon(sri.Stream);
            _notifyIcon.Visible = true;
            _notifyIcon.MouseClick += (_, __) => {
                WindowState = WindowState.Normal;
                Activate();
            };
        }

        protected override void OnStateChanged(EventArgs e) {
            base.OnStateChanged(e);

            ShowInTaskbar = WindowState != WindowState.Minimized;
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            if (_notifyIcon != null) {
                _notifyIcon.Visible = false;
                _notifyIcon.Icon?.Dispose();
                _notifyIcon.Dispose();
            }

            if (spotifyHook != null) {
                spotifyHook.Deactivate();
                spotifyHook.Dispose();
            }
        }

        private void OpenVolumeControlButton_Click(object sender, RoutedEventArgs e) {
            VolumeMixer.Open();
        }

        private void SpotifyHook_SpotifyStateChanged(object sender, EventArgs eventArgs) {
            Debug.WriteLine($"State change: hooked={spotifyHook.IsHooked}, state={spotifyHook.State}");
            Dispatcher.Invoke(() => {
                UpdateStatusLabel();
                UpdateMuteStatus();
            });
        }

        private void UpdateMuteStatus() {
            spotifyHook.SetMute(mute: !spotifyHook.IsSongPlaying);
        }

        private void UpdateStatusLabel() {
            if (!spotifyHook.IsHooked) {
                StatusLabel.Text = "Spotify is inactive";
            } else if (spotifyHook.IsPaused) {
                StatusLabel.Text = "Spotify is paused";
            } else if (spotifyHook.IsAdPlaying) {
                StatusLabel.Text = "Ad is playing";
            } else if (spotifyHook.IsSongPlaying && spotifyHook.ActiveSong is SongInfo song) {
                StatusLabel.Text = $"Playing {song.Title} by {song.Artist}";
            } else {
                StatusLabel.Text = "Unknown State";
            }
        }
    }
}
