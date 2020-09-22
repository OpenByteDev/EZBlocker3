using EZBlocker3.Interop;
using EZBlocker3.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace EZBlocker3.AutoUpdate {
    public partial class UpdateWindow : Window {

        private UpdateInfo Update;

        public UpdateWindow(UpdateInfo update) {
            Update = update;
            InitializeComponent();

            acceptDownloadButton.Click += (_, __) => {
                acceptDownloadButton.IsEnabled = false;
                Task.Run(DownloadUpdate);
            };
        }

        private async Task DownloadUpdate() {
            var download = new UpdateDownloader(Update);
            download.Progress += (s, e) => {
                Dispatcher.Invoke(() => {
                    var normalizedPercentage = e.DownloadPercentage;
                    var percentage = normalizedPercentage * 100;
                    downloadProgress.Value = percentage;
                    TaskbarItemInfo = new TaskbarItemInfo() {
                        ProgressValue = normalizedPercentage,
                        ProgressState = TaskbarItemProgressState.Normal
                    };
                });
            };

            DownloadedUpdate? downloadedUpdate = null;
            try {
                downloadedUpdate = await download.Run();
            } catch(Exception e) {
                Logger.LogError("AutoUpdate: Update download failed:\n" + e);
                Close();
                return;
            }

            Dispatcher.Invoke(() => {
                restartButton.IsEnabled = true;
                restartButton.Click += (_, __) => {
                    TaskbarItemInfo = new TaskbarItemInfo() {
                        ProgressState = TaskbarItemProgressState.Indeterminate
                    };
                    UpdateInstaller.InstallUpdateAndRestart(downloadedUpdate);
                    restartButton.IsEnabled = false;
                };
                TaskbarItemInfo = new TaskbarItemInfo() {
                    ProgressValue = 0,
                    ProgressState = TaskbarItemProgressState.None
                };
                TaskbarItemFlashHelper.FlashUntilFocused(this);
            });
        }
    }
}
