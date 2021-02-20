using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using EZBlocker3.Audio;
using EZBlocker3.AutoUpdate;
using EZBlocker3.Extensions;
using EZBlocker3.Logging;
using EZBlocker3.Settings;
using EZBlocker3.Spotify;
using Microsoft.Windows.Sdk;
using static EZBlocker3.AutoUpdate.UpdateFoundWindow;
using Application = System.Windows.Application;
using Brush = System.Windows.Media.Brush;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace EZBlocker3 {
    public partial class MainWindow : Window {
        private SpotifyHandler spotifyHandler;
        private NotifyIcon notifyIcon;
        private readonly CancellationTokenSource cancellationSource = new();

#pragma warning disable CS8618
        public MainWindow() {
#pragma warning restore CS8618
            InitializeComponent();

            // add version to window title
            var version = App.Version;
            Title += $" v{version}";

            // recolor window in debug mode
            Properties.Settings.Default.PropertyChanged += (s, e) => {
                if (e.PropertyName != nameof(Properties.Settings.DebugMode))
                    return;
                UpdateBorderBrush();
            };
            UpdateBorderBrush();

            SetupSpotifyHook();
            SetupNotifyIcon();

            OpenVolumeControlButton.Click += OpenVolumeControlButton_Click;
            SettingsIcon.MouseUp += SettingsIcon_MouseUp;

            UpdateStatusLabel();

            Loaded += MainWindow_Loaded;

            // if the screen configuration did not change, we restore the window position
            if (new Size(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight) == Properties.Settings.Default.VirtualScreenSize
                && Properties.Settings.Default.MainWindowPosition is Point position && position.X >= 0 && position.Y >= 0) {
                (Left, Top) = position;
            }

            MaybePerformUpdateCheck();
        }

        #region WindowProc
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            var source = (HwndSource)PresentationSource.FromDependencyObject(this);
            source.AddHook(WindowProc);
        }
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case Constants.WM_SYSCOMMAND:
                    if ((int)wParam == Constants.SC_CLOSE) {
                        // we manually close here, so that the "close window" command in the taskbar keeps working even if there is a dialog open.
                        Close();
                    }
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region AutoUpdate
        // is there a better name for this?
        private void MaybePerformUpdateCheck() {
            if (App.ForceUpdateCheck) {
                Task.Run(RunUpdateCheck, cancellationSource.Token);
                return;
            }

            if (!Properties.Settings.Default.CheckForUpdates)
                return;

            var now = DateTime.Now;
            if (Properties.Settings.Default.LastUpdateCheck is DateTime lastUpdateCheckDate) {
                var timeSinceLastUpdateCheck = now - lastUpdateCheckDate;
                if (timeSinceLastUpdateCheck < TimeSpan.FromDays(1))
                    return;
            }
            Properties.Settings.Default.LastUpdateCheck = now;

            Task.Run(RunUpdateCheck, cancellationSource.Token);
        }
        private async Task RunUpdateCheck() {
            try {
                var result = await UpdateChecker.CheckForUpdate(cancellationSource.Token).ConfigureAwait(false);
                if (result is not UpdateInfo update) // No update found
                    return;

                if (Properties.Settings.Default.IgnoreUpdate == update.UpdateVersion.ToString())
                    return;

                await Dispatcher.InvokeAsync(() => {
                    switch (ShowUpdateFoundWindow(update)) {
                        case UpdateDecision.Accept:
                            Logger.AutoUpdate.LogInfo($"Accepted update to {update.UpdateVersion}");
                            ShowDownloadWindow(update);
                            break;
                        case UpdateDecision.NotNow:
                            Logger.AutoUpdate.LogInfo($"Delayed update to {update.UpdateVersion}");
                            break;
                        case UpdateDecision.IgnoreUpdate:
                            Logger.AutoUpdate.LogInfo($"Ignored update to {update.UpdateVersion}");
                            Properties.Settings.Default.IgnoreUpdate = update.UpdateVersion.ToString();
                            break;
                    }
                }, cancellationSource.Token);
            } catch (Exception e) {
                Logger.AutoUpdate.LogException("Update operation failed", e);
            }
        }

        private UpdateDecision? ShowUpdateFoundWindow(UpdateInfo update) {
            var updateWindow = new UpdateFoundWindow(update) {
                Owner = this
            };
            updateWindow.ShowDialog();
            return updateWindow.Decision;
        }

        private bool? ShowDownloadWindow(UpdateInfo update) {
            var downloadUpdateWindow = new DownloadUpdateWindow(update) {
                Owner = this
            };
            return downloadUpdateWindow.ShowDialog();
        }
        #endregion

        #region Settings
        private void SettingsIcon_MouseUp(object sender, MouseButtonEventArgs e) {
            ShowSettingsWindow();
        }

        private bool? ShowSettingsWindow() {
            var settingsWindow = new SettingsWindow {
                Owner = this
            };
            return settingsWindow.ShowDialog();
        }

        #endregion

        #region NotifyIcon
        private void SetupNotifyIcon() {
            notifyIcon = new NotifyIcon();

            var sri = Application.GetResourceStream(new Uri("/Icon/Icon32.ico", UriKind.Relative));
            if (sri != null)
                notifyIcon.Icon = new Icon(sri.Stream);
            notifyIcon.Visible = true;
            notifyIcon.MouseClick += (_, e) => {
                switch (e.Button) {
                    case MouseButtons.Left:
                        Deminimize();
                        break;
                    case MouseButtons.Right:
                        ShowNotifyIconContextMenu();
                        break;
                }
            };
            notifyIcon.BalloonTipClicked += (_, __) => Deminimize();
            StateChanged += (_, __) => {
                if (WindowState == WindowState.Minimized && !ShowInTaskbar)
                    notifyIcon.ShowBalloonTip(5000, "EZBlocker3", "EZBlocker3 is hidden. Click this icon to restore.", ToolTipIcon.None);
            };
            // ensure context menu gets closed.
            Closed += (_, __) => {
                CloseNotifyIconContextMenu();
                if (_notifyIconContextMenu != null)
                    _notifyIconContextMenu.Visibility = Visibility.Hidden;
            };
        }

        private ContextMenu? _notifyIconContextMenu;
        private void ShowNotifyIconContextMenu() {
            if (_notifyIconContextMenu is null) {
                _notifyIconContextMenu = new ContextMenu();

                var showWindowItem = new MenuItem {
                    Header = "Show Application Window"
                };
                showWindowItem.Click += (_, __) => Deminimize();
                _notifyIconContextMenu.Items.Add(showWindowItem);

                var openProjectPageItem = new MenuItem {
                    Header = "Open Project Page"
                };
                openProjectPageItem.Click += (_, __) => {
                    Process.Start(new ProcessStartInfo() {
                        FileName = Properties.Resources.ProjectPageUrl,
                        UseShellExecute = true
                    });
                };
                _notifyIconContextMenu.Items.Add(openProjectPageItem);

                var exitItem = new MenuItem {
                    Header = "Exit"
                };
                exitItem.Click += (_, __) => Application.Current.Shutdown();

                _notifyIconContextMenu.Items.Add(exitItem);
            }
            _notifyIconContextMenu.IsOpen = true;
        }
        private void CloseNotifyIconContextMenu() {
            if (_notifyIconContextMenu is null)
                return;
            _notifyIconContextMenu.IsOpen = false;
        }
        #endregion

        #region SpotifyHook
        private void SetupSpotifyHook() {
            spotifyHandler = new();

            spotifyHandler.Hook.SpotifyStateChanged += (_, __) => UpdateStatusLabel();
            spotifyHandler.Hook.ActiveSongChanged += (_, __) => UpdateStatusLabel();
            spotifyHandler.Hook.HookChanged += (_, __) => UpdateStatusLabel();

            spotifyHandler.Activate();
        }

        private void UpdateStatusLabel() {
            Dispatcher.BeginInvoke(() => StateLabel.Text = GetStateText());
        }

        private string GetStateText() {
            if (!spotifyHandler.Hook.IsHooked) {
                return "Spotify is not running.";
            } else {
                switch (spotifyHandler.Hook.State) {
                    case SpotifyState.Paused:
                        return "Spotify is paused.";
                    case SpotifyState.PlayingSong:
                        if (spotifyHandler.Hook.ActiveSong is not SongInfo song) {
                            Logger.Hook.LogError("Active song is undefined in PlayingSong state.");
                            throw new IllegalStateException();
                        }
                        return $"Playing {song.Title} by {song.Artist}";
                    case SpotifyState.PlayingAdvertisement:
                        return "Playing advertisement...";
                    case SpotifyState.StartingUp:
                        return "Spotify is starting...";
                    case SpotifyState.ShuttingDown:
                        return "Spotify is shutting down...";
                    // case SpotifyState.Unknown:
                    default:
                        return "Spotify is an unknown state.";
                }
            }
        }
        #endregion

        private void OpenVolumeControlButton_Click(object sender, RoutedEventArgs e) {
            Task.Run(() => VolumeMixer.Open(), cancellationSource.Token);
        }

        public void Minimize() {
            WindowState = WindowState.Minimized;

            OnStateChanged(EventArgs.Empty);
        }
        public void Deminimize() {
            WindowState = WindowState.Normal;
            Show();
            Activate();

            OnStateChanged(EventArgs.Empty);
        }

        private (Brush, Thickness)? _defaultBorder;
        private static readonly (Brush, Thickness) _debugModeBorder = (new SolidColorBrush(Colors.OrangeRed), new Thickness(2));
        private void UpdateBorderBrush() {
            (BorderBrush, BorderThickness) = App.DebugModeEnabled ? _debugModeBorder : (_defaultBorder ??= (BorderBrush, BorderThickness));
        }

        protected override void OnStateChanged(EventArgs e) {
            if (Properties.Settings.Default.MinimizeToTray)
                ShowInTaskbar = WindowState != WindowState.Minimized;

            base.OnStateChanged(e);
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            if (WindowState == WindowState.Normal) {
                if (!App.SaveSettingsOnClose)
                    return;

                if (Left >= 0 && Top >= 0) {
                    Properties.Settings.Default.MainWindowPosition = new Point(Left, Top);
                    Properties.Settings.Default.VirtualScreenSize = new Size(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
                }
                Properties.Settings.Default.Save();
            }
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            cancellationSource.Cancel();

            if (Properties.Settings.Default.UnmuteOnClose)
                spotifyHandler.Muter?.Unmute();

            if (notifyIcon != null) {
                notifyIcon.Visible = false;
                notifyIcon.Icon?.Dispose();
                notifyIcon.Dispose();
            }

            spotifyHandler.Deactivate();
            spotifyHandler.Dispose();
        }
    }
}
