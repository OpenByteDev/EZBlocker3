using System.Windows;
using MessageBox = ModernWpf.MessageBox;

namespace EZBlocker3.Settings {
    public partial class SettingsWindow : Window {

        public SettingsWindow() {
            InitializeComponent();

            unmuteOnCloseCheckBox.IsChecked = Properties.Settings.Default.UnmuteOnClose;
            minimizeToTrayRadioButton.IsChecked = Properties.Settings.Default.MinimizeToTray;
            minimizeToTaskbarRadioButton.IsChecked = !Properties.Settings.Default.MinimizeToTray;
            checkForUpdatesCheckBox.IsChecked = Properties.Settings.Default.CheckForUpdates;
            debugModeCheckBox.IsChecked = Properties.Settings.Default.DebugMode;
            assumeAdOnUnknowsStateCheckBox.IsChecked = Properties.Settings.Default.AssumeAdOnUnknownState;
            startMinimizedCheckBox.IsChecked = Properties.Settings.Default.StartMinimized;
            startOnLoginCheckBox.IsChecked = Properties.Settings.Default.StartOnLogin;
            startWithSpotifyCheckBox.IsChecked = Properties.Settings.Default.StartWithSpotify;

            startWithSpotifyCheckBox.IsEnabled = StartWithSpotify.Available;

            saveButton.Click += (_, __) => { SaveSettings(); Close(); };
            cancelButton.Click += (_, __) => { Close(); };
            uninstallButton.Click += (_, __) => {
                if (MessageBox.Show("Do you really want to uninstall EZBlocker 3?", "Confirm Uninstall", MessageBoxButton.YesNo) == MessageBoxResult.Yes) {
                    Uninstall.Run();
                }
            };
        }

        private void SaveSettings() {
            if (!App.SaveSettingsOnClose)
                return;

            Properties.Settings.Default.UnmuteOnClose = unmuteOnCloseCheckBox.IsChecked ?? Properties.Settings.Default.UnmuteOnClose;
            Properties.Settings.Default.MinimizeToTray = minimizeToTrayRadioButton.IsChecked ?? Properties.Settings.Default.MinimizeToTray;
            Properties.Settings.Default.CheckForUpdates = checkForUpdatesCheckBox.IsChecked ?? Properties.Settings.Default.CheckForUpdates;
            Properties.Settings.Default.DebugMode = debugModeCheckBox.IsChecked ?? Properties.Settings.Default.DebugMode;
            Properties.Settings.Default.AssumeAdOnUnknownState = assumeAdOnUnknowsStateCheckBox.IsChecked ?? Properties.Settings.Default.AssumeAdOnUnknownState;
            Properties.Settings.Default.StartMinimized = startMinimizedCheckBox.IsChecked ?? Properties.Settings.Default.StartMinimized;
            Properties.Settings.Default.StartOnLogin = startOnLoginCheckBox.IsChecked ?? Properties.Settings.Default.StartOnLogin;
            Properties.Settings.Default.StartWithSpotify = startWithSpotifyCheckBox.IsChecked ?? Properties.Settings.Default.StartWithSpotify;

            Autostart.SetEnabled(Properties.Settings.Default.StartOnLogin);
            if (StartWithSpotify.Available)
                StartWithSpotify.SetEnabled(Properties.Settings.Default.StartWithSpotify);

            Properties.Settings.Default.Save();
        }

    }
}
