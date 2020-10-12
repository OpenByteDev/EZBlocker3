using EZBlocker3.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;

namespace EZBlocker3.Settings {
    public static class Uninstall {

        public static void Run(bool deleteSettings = false) {
            Autostart.Disable();
            StartWithSpotify.Disable();

            if (deleteSettings) {
                App.SaveSettingsOnClose = false;
                DeleteSettings();
            }

            Process.Start(new ProcessStartInfo() {
                FileName = "cmd.exe",
                Arguments = "/C choice /C Y /N /D Y /T 5 & DEL " + App.Location,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
            });

            Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown());

            void DeleteSettings() {
                var appExecutableName = Path.GetFileName(App.Location);
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var containerPath = Path.Combine(appDataPath, App.CompanyName);
                var containerDiretory = new DirectoryInfo(containerPath);

                var settingsDirectories = containerDiretory.GetDirectories()
                    .Where(directory => directory.Name.StartsWith(appExecutableName));

                foreach (var settingsDir in settingsDirectories)
                    settingsDir.RecursiveDelete();

                if (!containerDiretory.EnumerateDirectories().Any() && !containerDiretory.EnumerateFiles().Any())
                    containerDiretory.Delete();
            }
        }

    }
}
