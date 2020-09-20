using EZBlocker3.Logging;
using System;
using System.Threading.Tasks;
using System.Windows;

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
                    downloadProgress.Value = e.DownloadPercentage * 100;
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
                    UpdateInstaller.InstallUpdateAndRestart(downloadedUpdate);
                    restartButton.IsEnabled = false;
                };
            });
        }
    }
}
