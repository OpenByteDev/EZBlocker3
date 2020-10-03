using System.Windows;

namespace EZBlocker3.Settings {
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();

            unmuteOnCloseCheckBox.IsChecked = Properties.Settings.Default.UnmuteOnClose;
            minimizeToTrayRadioButton.IsChecked = Properties.Settings.Default.MinimizeToTray;
            minimizeToTaskbarRadioButton.IsChecked = !Properties.Settings.Default.MinimizeToTray;
            checkForUpdatesCheckBox.IsChecked = Properties.Settings.Default.CheckForUpdates;
            debugModeCheckBox.IsChecked = Properties.Settings.Default.DebugMode;
            startMinimizedCheckBox.IsChecked = Properties.Settings.Default.StartMinimized;
            startOnLogin.IsChecked = Properties.Settings.Default.StartOnLogin;

            saveButton.Click += (_, __) => { SaveSettings(); Close(); };
            cancelButton.Click += (_, __) => { Close(); };
        }

        private void SaveSettings() {
            Properties.Settings.Default.UnmuteOnClose = unmuteOnCloseCheckBox.IsChecked ?? Properties.Settings.Default.UnmuteOnClose;
            Properties.Settings.Default.MinimizeToTray = minimizeToTrayRadioButton.IsChecked ?? Properties.Settings.Default.MinimizeToTray;
            Properties.Settings.Default.CheckForUpdates = checkForUpdatesCheckBox.IsChecked ?? Properties.Settings.Default.CheckForUpdates;
            Properties.Settings.Default.DebugMode = debugModeCheckBox.IsChecked ?? Properties.Settings.Default.DebugMode;
            Properties.Settings.Default.StartMinimized = startMinimizedCheckBox.IsChecked ?? Properties.Settings.Default.StartMinimized;
            Properties.Settings.Default.StartOnLogin = startOnLogin.IsChecked ?? Properties.Settings.Default.StartOnLogin;

            Autostart.SetEnabled(Properties.Settings.Default.StartOnLogin);

            Properties.Settings.Default.Save();
        }

    }
}
