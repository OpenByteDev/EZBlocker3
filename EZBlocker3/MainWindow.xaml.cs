using System;
using System.Windows;

namespace EZBlocker3 {
    public partial class MainWindow : Window {

        private readonly SpotifyHook spotifyHook = new SpotifyHook();

        public MainWindow() {
            InitializeComponent();

            OpenVolumeControlButton.Click += OpenVolumeControlButton_Click;
            // MuteSpotifyButton.Click += MuteSpotifyButton_Click;

            spotifyHook.Activate();
            spotifyHook.ActiveSongChanged += SpotifyHook_ActiveSongChanged;

            UpdateStatusLabel();
            UpdateMuteStatus();
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            spotifyHook?.Deactivate();
            spotifyHook?.Dispose();
        }

        private void OpenVolumeControlButton_Click(object sender, RoutedEventArgs e) {
            VolumeMixer.Open();
        }

        private void SpotifyHook_ActiveSongChanged(object sender, EventArgs eventArgs) {
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
