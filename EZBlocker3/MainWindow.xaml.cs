﻿using EZBlocker3.AutoUpdate;
using EZBlocker3.Extensions;
using EZBlocker3.Interop;
using EZBlocker3.Logging;
using EZBlocker3.Settings;
using EZBlocker3.Spotify;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using static EZBlocker3.AutoUpdate.UpdateFoundWindow;
using static EZBlocker3.SpotifyHook;
using Application = System.Windows.Application;
using ContextMenu = System.Windows.Controls.ContextMenu;
using MenuItem = System.Windows.Controls.MenuItem;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace EZBlocker3 {
    public partial class MainWindow : Window {

        private SpotifyHook _spotifyHook;
        private SpotifyMuter _spotifyMuter;
        private NotifyIcon _notifyIcon;
        private readonly CancellationTokenSource _cancellationSource = new CancellationTokenSource();

#pragma warning disable CS8618
        public MainWindow() {
            InitializeComponent();

            SetupSpotifyHook();
            SetupNotifyIcon();

            OpenVolumeControlButton.Click += OpenVolumeControlButton_Click;
            SettingsIcon.MouseDown += SettingsIcon_MouseDown;

            UpdateStatusLabel();

            Loaded += MainWindow_Loaded;

            // if the screen configuration did not change, we restore the window position
            if (new Size(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight) == Properties.Settings.Default.VirtualScreenSize)
                (Left, Top) = Properties.Settings.Default.MainWindowPosition;

            if (Properties.Settings.Default.StartMinimized)
                Minimize();

            if (Properties.Settings.Default.CheckForUpdates)
                Task.Run(RunUpdateCheck, _cancellationSource.Token);

        }
#pragma warning restore CS8618

        #region WindowProc
        private void MainWindow_Loaded(object sender, RoutedEventArgs e) {
            var source = (HwndSource)PresentationSource.FromDependencyObject(this);
            source.AddHook(WindowProc);
        }
        private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (msg) {
                case (int)NativeMethods.WindowProcMessage.WM_SYSCOMMAND:
                    if ((int)wParam == (int)NativeMethods.WindowMessageSystemCommand.SC_CLOSE)
                        // we manually close here, so that the "close window" command in the taskbar keeps working even if there is a dialog open.
                        Close();
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region AutoUpdate
        private async Task RunUpdateCheck() {
            try {
                var result = await UpdateChecker.CheckForUpdate(_cancellationSource.Token);
                if (!(result is UpdateInfo update)) // No update found
                    return;

                if (Properties.Settings.Default.IgnoreUpdate == update.UpdateVersion.ToString())
                    return;

                Dispatcher.Invoke(() => {
                    var decision = ShowUpdateFoundWindow(update);
                    switch (decision) {
                        case UpdateDecision.Accept:
                            Logger.LogInfo($"AutoUpdate: Accepted update to {update.UpdateVersion}");
                            ShowDownloadWindow(update);
                            break;
                        case UpdateDecision.NotNow:
                            Logger.LogInfo($"AutoUpdate: Delayed update to {update.UpdateVersion}");
                            break;
                        case UpdateDecision.IgnoreUpdate:
                            Logger.LogInfo($"AutoUpdate: Ignored update to {update.UpdateVersion}");
                            Properties.Settings.Default.IgnoreUpdate = update.UpdateVersion.ToString();
                            break;
                    }
                });
            } catch (Exception e) {
                Logger.LogException("Auto update failed", e);
            }
        }

        private UpdateDecision? ShowUpdateFoundWindow(UpdateInfo update) {
            var updateWindow = new UpdateFoundWindow(update);
            updateWindow.Owner = this;
            updateWindow.ShowDialog();
            return updateWindow.Decision;
        }

        private bool? ShowDownloadWindow(UpdateInfo update) {
            var downloadUpdateWindow = new DownloadUpdateWindow(update);
            downloadUpdateWindow.Owner = this;
            return downloadUpdateWindow.ShowDialog();
        }
        #endregion

        #region Settings
        private void SettingsIcon_MouseDown(object sender, MouseButtonEventArgs e) {
            ShowSettingsWindow();
        }

        private bool? ShowSettingsWindow() {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            return settingsWindow.ShowDialog();
        }

        #endregion

        #region NotifyIcon
        private void SetupNotifyIcon() {
            _notifyIcon = new NotifyIcon();

            var sri = Application.GetResourceStream(new Uri("/Icon/Icon32.ico", UriKind.Relative));
            if (sri != null)
                _notifyIcon.Icon = new Icon(sri.Stream);
            _notifyIcon.Visible = true;
            _notifyIcon.MouseClick += (_, e) => {
                switch (e.Button) {
                    case MouseButtons.Left:
                        Deminimize();
                        break;
                    case MouseButtons.Right:
                        ShowNotifyIconContextMenu();
                        break;
                }
            };
        }

        private ContextMenu? _notifyIconContextMenu;
        private void ShowNotifyIconContextMenu() {
            if (_notifyIconContextMenu is null) {
                _notifyIconContextMenu = new ContextMenu();
                var openItem = new MenuItem {
                    Header = "Show Window"
                };
                openItem.Click += (_, __) => Deminimize();
                _notifyIconContextMenu.Items.Add(openItem);
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
            _spotifyHook = new SpotifyHook();
            _spotifyMuter = new SpotifyMuter(_spotifyHook);

            _spotifyHook.SpotifyStateChanged += (_, __) => {
                UpdateStatusLabel();
            };
            _spotifyHook.ActiveSongChanged += (_, __) => {
                UpdateStatusLabel();
            };
            _spotifyHook.HookChanged += (_, __) => {
                UpdateStatusLabel();
            };
            _spotifyHook.Activate();
        }


        private void UpdateStatusLabel() {
            Dispatcher.Invoke(() => {
                StateLabel.Text = GetStateText();
            });
        }

        private string GetStateText() {
            if (!_spotifyHook.IsHooked) {
                return "Spotify is not running.";
            } else {
                switch (_spotifyHook.State) {
                    case SpotifyState.Paused:
                        return "Spotify is paused.";
                    case SpotifyState.PlayingSong:
                        if (!(_spotifyHook.ActiveSong is SongInfo song)) {
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
        #endregion

        private void OpenVolumeControlButton_Click(object sender, RoutedEventArgs e) {
            Task.Run(VolumeMixer.Open, _cancellationSource.Token);
        }

        public void Minimize() {
            WindowState = WindowState.Minimized;
        }
        public void Deminimize() {
            WindowState = WindowState.Normal;
            Activate();
        }

        protected override void OnStateChanged(EventArgs e) {
            base.OnStateChanged(e);

            if (Properties.Settings.Default.MinimizeToTray)
                ShowInTaskbar = WindowState != WindowState.Minimized;
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            if (WindowState == WindowState.Normal) {
                Properties.Settings.Default.MainWindowPosition = new Point(Left, Top);
                Properties.Settings.Default.VirtualScreenSize = new Size(SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
                Properties.Settings.Default.Save();
            }
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            _cancellationSource.Cancel();

            CloseNotifyIconContextMenu();

            if (Properties.Settings.Default.UnmuteOnClose)
                _spotifyHook?.Unmute();

            if (_notifyIcon != null) {
                _notifyIcon.Visible = false;
                _notifyIcon.Icon?.Dispose();
                _notifyIcon.Dispose();
            }

            if (_spotifyHook != null) {
                _spotifyHook.Deactivate();
                _spotifyHook.Dispose();
            }

            GlobalSingletons.Dispose();
        }

    }
}
