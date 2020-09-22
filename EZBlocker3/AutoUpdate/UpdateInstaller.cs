using EZBlocker3.Logging;
using Ionic.Zip;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace EZBlocker3.AutoUpdate {
    public static class UpdateInstaller {

        public static void InstallUpdateAndRestart(DownloadedUpdate update) {
            // extract executable
            using var zip = ZipFile.Read(update.UpdateBytes);

            var appLocation = Assembly.GetExecutingAssembly().Location;
            var appDirectory = Path.GetDirectoryName(appLocation);
            var tempOldAppPath = Path.ChangeExtension(appLocation, ".exe.bak");
            var tempNewAppPath = Path.ChangeExtension(appLocation, ".exe.update");

            var exeEntry = FindExecutableEntry(zip);
            using (var tempNewAppFile = File.OpenWrite(tempNewAppPath)) {
                exeEntry.Extract(tempNewAppFile);
            }

            Logger.LogDebug("AutoUpdate: Extracted update");

            File.Delete(tempOldAppPath);
            File.Move(appLocation, tempOldAppPath);
            File.Move(tempNewAppPath, appLocation);

            Logger.LogDebug("AutoUpdate: Replaced executable");

            Logger.LogDebug("AutoUpdate: Restarting");

            // Process.Start(appLocation, "/updateRestart");
            Process.Start(new ProcessStartInfo() {
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = "cmd.exe",
                Arguments = $"/C TIMEOUT /T 3 /NOBREAK & START \"\" \"{appLocation}\" /updateRestart",
                UseShellExecute = true,
                CreateNoWindow = true
            });
            Application.Current.Dispatcher.Invoke(() => {
                Application.Current.Shutdown();
            });
        }

        private static ZipEntry FindExecutableEntry(ZipFile zipFile) {
            return zipFile.Entries.FirstOrDefault(zip => !zip.IsDirectory && zip.FileName == "EZBlocker3.exe");
        }

    }
}
