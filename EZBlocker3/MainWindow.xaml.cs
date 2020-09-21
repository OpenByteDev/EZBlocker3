using EZBlocker3.AutoUpdate;
using EZBlocker3.Logging;
using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using static EZBlocker3.SpotifyHook;
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
            // UpdateMuteStatus();

            Task.Run(() => RunUpdateCheck());
        }

        private async Task RunUpdateCheck() {
            try {
                var result = await UpdateChecker.CheckForUpdate();
                if (!(result is UpdateInfo update)) // No update found
                    return;

                Dispatcher.Invoke(() => {
                    var updateWindow = new UpdateWindow(update);
                    updateWindow.Show();
                    Closed += (_, __) => updateWindow.Close();
                });
            } catch (Exception e) {
                Logger.LogError($"Auto update failed: {e}");
            }
        }

        private void SetupSpotifyHook() {
            spotifyHook.SpotifyStateChanged += (_, __) => SpotifyHookStateChanged();
            spotifyHook.ActiveSongChanged += (_, __) => SpotifyHookStateChanged();
            spotifyHook.HookChanged += (_, __) => SpotifyHookStateChanged();
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
            Task.Run(() => VolumeMixer.Open());
        }

        private void SpotifyHookStateChanged() {
            Dispatcher.Invoke(() => {
                UpdateStatusLabel();
                UpdateMuteStatus();
            });
        }

        private void UpdateMuteStatus() =>
            spotifyHook.SetMute(mute: spotifyHook.IsAdPlaying);

        private void UpdateStatusLabel() {
            StateLabel.Text = GetStateText();
            
        }

        private string GetStateText() {
            if (!spotifyHook.IsHooked) {
                return "Spotify is not running.";
            } else {
                switch (spotifyHook.State) {
                    case SpotifyState.Paused:
                        return "Spotify is paused.";
                    case SpotifyState.PlayingSong:
                        if (!(spotifyHook.ActiveSong is SongInfo song)) {
                            Logger.LogError("SpotifyHook: Active song is undefined in PlayingSong state.");
                            throw new IllegalStateException();
                        }
                        return $"Playing {song.Title} by {song.Artist}";
                    case SpotifyState.PlayingAdvertisement:
                        return "Playing advertisement...";
                    case SpotifyState.StartingUp:
                        return "Spotify is starting...";
                    case SpotifyState.ShuttingDown:
                        return "Spotify is shutting down...";
                    case SpotifyState.Unknown:
                    default:
                        return "Spotify is an unknown state.";
                }
            }
        }
    }
}
