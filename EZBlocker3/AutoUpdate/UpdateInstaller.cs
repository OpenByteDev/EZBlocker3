using EZBlocker3.Logging;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace EZBlocker3.AutoUpdate {
    public static class UpdateInstaller {

        private static string? _tempOldAppPath;
        private static string TempOldAppPath => _tempOldAppPath ??= Path.ChangeExtension(App.Location, ".exe.bak");

        private static string? _tempNewAppPath;
        private static string TempNewAppPath => _tempNewAppPath ??= Path.ChangeExtension(App.Location, ".exe.bak");

        public static void InstallUpdateAndRestart(DownloadedUpdate update) {
            try {
                Logger.LogDebug("AutoUpdate: Begin install");

                using (var tempNewAppFile = File.OpenWrite(TempNewAppPath))
                    update.UpdateBytes.WriteTo(tempNewAppFile);
                update.Dispose();

                Logger.LogDebug("AutoUpdate: Extracted update");

                File.Delete(TempOldAppPath);
                File.Move(App.Location, TempOldAppPath);
                File.Move(TempNewAppPath, App.Location);

                Logger.LogDebug("AutoUpdate: Replaced executable");

                Logger.LogDebug("AutoUpdate: Restarting");
                Process.Start(App.Location, "/updateRestart");
                Application.Current.Dispatcher.Invoke(() => {
                    Application.Current.Shutdown();
                });
            } catch(Exception e) {
                Logger.LogException("AutoUpdate: Installation failed:", e);
                Logger.LogInfo("AutoUpdate: Starting failure cleanup");
                // cleanup failed installation

                // try to restore app executable
                if (!File.Exists(App.Location)) {
                    if (File.Exists(TempOldAppPath))
                        File.Move(TempOldAppPath, App.Location);
                    else if (File.Exists(TempNewAppPath))
                        File.Move(TempNewAppPath, App.Location);
                }

                // delete update file if it still exists
                File.Delete(TempNewAppPath);

                Logger.LogInfo("AutoUpdate: Finished failure cleanup");

                // rethrow exception
                throw;
            }
        }

        public static void CleanupUpdate() {
            File.Delete(TempOldAppPath);
        }
    }
}
