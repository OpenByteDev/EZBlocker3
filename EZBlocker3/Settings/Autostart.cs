using Microsoft.Win32;

namespace EZBlocker3.Settings {
    public static class Autostart {

        private const string StartupApprovedRunKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public static bool? IsEnabled() {
            using var runKey = Registry.CurrentUser.OpenSubKey(RunKey, writable: false);
            var runKeyValue = runKey?.GetValue(App.ProductName);

            using var startupApprovedRunKey = Registry.CurrentUser.OpenSubKey(StartupApprovedRunKey, writable: false);
            var startupApprovedRunKeyValue = startupApprovedRunKey?.GetValue(App.ProductName);

            return ((runKeyValue, startupApprovedRunKeyValue)) switch {
                (not null, not null) => true,
                (null, null) => false,
                _ => null
            };
        }

        public static void Enable() {
            using var runKey = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            runKey?.SetValue(App.ProductName, App.Location, RegistryValueKind.String);

            using var startupApprovedRunKey = Registry.CurrentUser.OpenSubKey(StartupApprovedRunKey, writable: true);
            startupApprovedRunKey?.SetValue(App.ProductName, new byte[] { 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, RegistryValueKind.Binary);
        }

        public static void Disable() {
            using var runKey = Registry.CurrentUser.OpenSubKey(RunKey, writable: true);
            runKey?.DeleteValue(App.ProductName);

            using var startupApprovedRunKey = Registry.CurrentUser.OpenSubKey(StartupApprovedRunKey, writable: true);
            startupApprovedRunKey?.DeleteValue(App.ProductName);
        }

        public static void SetEnabled(bool enabled) {
            if (enabled)
                Enable();
            else
                Disable();
        }
    }
}
