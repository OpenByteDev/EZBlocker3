using EZBlocker3.Extensions;
using EZBlocker3.Interop;
using EZBlocker3.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shell;

namespace EZBlocker3.AutoUpdate {
    public partial class UpdateWindow : Window {

        private readonly UpdateInfo Update;

        public UpdateWindow(UpdateInfo update) {
            Update = update;
            InitializeComponent();

            acceptDownloadButton.Click += (_, __) => {
                acceptDownloadButton.IsEnabled = false;
                Task.Run(DownloadUpdate);
            };
        }

        private async Task DownloadUpdate() {
            var download = new UpdateDownloader();
            download.Progress += (s, e) => {
                Dispatcher.Invoke(() => {
                    var normalizedPercentage = e.DownloadPercentage;
                    var percentage = normalizedPercentage * 100;
                    downloadProgress.Value = percentage;
                    downloadState.Text = $"{Math.Round(percentage)}%";
                    TaskbarItemInfo = new TaskbarItemInfo() {
                        ProgressValue = normalizedPercentage,
                        ProgressState = TaskbarItemProgressState.Normal
                    };
                });
            };

            DownloadedUpdate? downloadedUpdate = null;
            try {
                downloadedUpdate = await download.Download(Update);
            } catch (Exception e) {
                Logger.LogError("AutoUpdate: Update download failed:\n" + e);
                Dispatcher.Invoke(() => {
                    downloadState.Text = $"Download failed";
                });
                return;
            }

            Dispatcher.Invoke(() => {
                downloadState.Text = $"Download finished";
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
