using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using EZBlocker3.Logging;
using EZBlocker3.Settings;
using Lazy;

namespace EZBlocker3.AutoUpdate {
    public static class UpdateInstaller {
        [Lazy]
        private static string TempOldAppPath => Path.ChangeExtension(App.Location, ".exe.bak");

        [Lazy]
        private static string TempNewAppPath => Path.ChangeExtension(App.Location, ".exe.upd");

        public static void InstallUpdateAndRestart(DownloadedUpdate update) {
            try {
                Logger.AutoUpdate.LogDebug("Begin install");

                File.Delete(TempNewAppPath);
                using (var tempNewAppFile = File.OpenWrite(TempNewAppPath))
                    update.UpdateBytes.WriteTo(tempNewAppFile);

                Logger.AutoUpdate.LogDebug("Extracted update");

                File.Delete(TempOldAppPath);
                File.Move(App.Location, TempOldAppPath);
                File.Move(TempNewAppPath, App.Location);

                Logger.AutoUpdate.LogDebug("Replaced executable");

                // disable settings with external state before upgrade, as the new version maybe uses a different system.
                StartWithSpotify.Disable();
                Autostart.Disable();

                Logger.AutoUpdate.LogDebug("Restarting");
                Process.Start(App.Location, "/updateRestart").Dispose();
                Application.Current.Dispatcher.Invoke(() => Application.Current.Shutdown());
            } catch (Exception e) {
                Logger.AutoUpdate.LogException("Installation failed:", e);
                Logger.AutoUpdate.LogInfo("Starting failure cleanup");
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

                Logger.AutoUpdate.LogInfo("Finished failure cleanup");

                // rethrow exception
                throw;
            } finally {
                update.Dispose();
            }
        }

        public static void CleanupUpdate() {
            Task.Run(async () => {
                await Task.Delay(10000).ConfigureAwait(false);
                File.Delete(TempOldAppPath);
            });
        }
    }
}
