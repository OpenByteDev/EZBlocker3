using EZBlocker3.Logging;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace EZBlocker3.AutoUpdate {
    public static class UpdateInstaller {

        public static void InstallUpdateAndRestart(DownloadedUpdate update) {
            Logger.LogDebug("AutoUpdate: Begin install");

            var appLocation = App.Location;
            var appDirectory = Path.GetDirectoryName(appLocation);
            var tempOldAppPath = Path.ChangeExtension(appLocation, ".exe.bak");
            var tempNewAppPath = Path.ChangeExtension(appLocation, ".exe.upd");

            using (var tempNewAppFile = File.OpenWrite(tempNewAppPath))
                update.UpdateBytes.WriteTo(tempNewAppFile);
            update.Dispose();

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

    }
}
