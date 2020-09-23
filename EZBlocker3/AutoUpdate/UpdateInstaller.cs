using EZBlocker3.Extensions;
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
            Logger.LogDebug("AutoUpdate: Begin install");

            using var zip = ZipFile.Read(update.UpdateBytes);

            var appLocation = Assembly.GetExecutingAssembly().Location;
            var appDirectory = Path.GetDirectoryName(appLocation);
            var tempOldAppPath = Path.ChangeExtension(appLocation, ".exe.bak");
            var tempNewAppPath = Path.ChangeExtension(appLocation, ".exe.upd");

            var exeEntry = FindExecutableEntry(zip);
            exeEntry.ExtractTo(tempNewAppPath);

            Logger.LogDebug("AutoUpdate: Extracted update");

            File.Delete(tempOldAppPath);
            File.Move(appLocation, tempOldAppPath);
            File.Move(tempNewAppPath, appLocation);

            Logger.LogDebug("AutoUpdate: Replaced executable");

            Logger.LogDebug("AutoUpdate: Restarting");
            Process.Start(appLocation, "/updateRestart");
            Application.Current.Dispatcher.Invoke(() => {
                Application.Current.Shutdown();
            });
        }

        private static ZipEntry FindExecutableEntry(ZipFile zipFile) {
            return zipFile.Entries.FirstOrDefault(zip => !zip.IsDirectory && zip.FileName == "EZBlocker3.exe");
        }

    }
}
