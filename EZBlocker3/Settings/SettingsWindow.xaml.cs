using System.Windows;
using System;
using System.ComponentModel;
using Microsoft.Win32;

namespace EZBlocker3.Settings {
    public partial class SettingsWindow : Window {
        public SettingsWindow() {
            InitializeComponent();

            unmuteOnCloseCheckBox.IsChecked = Properties.Settings.Default.UnmuteOnClose;
            unmuteOnCloseCheckBox.Checked += (_, __) => Properties.Settings.Default.UnmuteOnClose = true;
            unmuteOnCloseCheckBox.Unchecked += (_, __) => Properties.Settings.Default.UnmuteOnClose = false;

            minimizeToTrayRadioButton.IsChecked = Properties.Settings.Default.MinimizeToTray;
            minimizeToTrayRadioButton.Checked += (_, __) => Properties.Settings.Default.MinimizeToTray = true;
            minimizeToTrayRadioButton.Unchecked += (_, __) => Properties.Settings.Default.MinimizeToTray = false;

            minimizeToTaskbarRadioButton.IsChecked = !Properties.Settings.Default.MinimizeToTray;
            minimizeToTaskbarRadioButton.Checked += (_, __) => Properties.Settings.Default.MinimizeToTray = false;
            minimizeToTaskbarRadioButton.Unchecked += (_, __) => Properties.Settings.Default.MinimizeToTray = true;

            checkForUpdatesCheckBox.IsChecked = Properties.Settings.Default.CheckForUpdates;
            checkForUpdatesCheckBox.Checked += (_, __) => Properties.Settings.Default.CheckForUpdates = true;
            checkForUpdatesCheckBox.Unchecked += (_, __) => Properties.Settings.Default.CheckForUpdates = false;

            debugModeCheckBox.IsChecked = Properties.Settings.Default.DebugMode;
            debugModeCheckBox.Checked += (_, __) => Properties.Settings.Default.DebugMode = true;
            debugModeCheckBox.Unchecked += (_, __) => Properties.Settings.Default.DebugMode = false;

            startMinimizedCheckBox.IsChecked = Properties.Settings.Default.StartMinimized;
            startMinimizedCheckBox.Checked += (_, __) => Properties.Settings.Default.StartMinimized = true;
            startMinimizedCheckBox.Unchecked += (_, __) => Properties.Settings.Default.StartMinimized = false;

            Autostart.SetEnabled(Properties.Settings.Default.StartOnLogin);
            startOnLogin.IsChecked = Properties.Settings.Default.StartOnLogin;
            startOnLogin.Checked += (_, __) => { Properties.Settings.Default.StartOnLogin = true; Autostart.Enable(); };
            startOnLogin.Unchecked += (_, __) => { Properties.Settings.Default.StartOnLogin = false; Autostart.Disable(); };
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            Properties.Settings.Default.Save();
        }

    }
}
