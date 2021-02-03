using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using EZBlocker3.Extensions;
using EZBlocker3.Logging;

namespace EZBlocker3.AutoUpdate {
    public partial class DownloadUpdateWindow : Window {
        private readonly UpdateInfo Update;
        private readonly CancellationTokenSource _cancellationSource = new();

        public DownloadUpdateWindow(UpdateInfo update) {
            Update = update;
            InitializeComponent();

            abortDownloadButton.Click += (_, __) => {
                _cancellationSource.Cancel();
                Close();
            };

            Task.Run(DownloadUpdate, _cancellationSource.Token);
        }

        protected override void OnClosed(EventArgs e) {
            base.OnClosed(e);

            _cancellationSource.Cancel();
        }

        private async Task DownloadUpdate() {
            var downloader = new UpdateDownloader();
            downloader.Progress += (s, e) => {
                Dispatcher.BeginInvoke(() => {
                    var normalizedPercentage = e.DownloadPercentage;
                    var percentage = normalizedPercentage * 100;
                    downloadProgress.Value = percentage;
                    downloadState.Text = $"Downloading... {Math.Round(percentage)}%";
                    Owner.TaskbarItemInfo = new TaskbarItemInfo() {
                        ProgressValue = normalizedPercentage,
                        ProgressState = TaskbarItemProgressState.Normal
                    };
                });
            };

            DownloadedUpdate? downloadedUpdate = null;
            try {
                downloadedUpdate = await downloader.Download(Update, _cancellationSource.Token).ConfigureAwait(false);
            } catch (Exception e) {
                Logger.LogException("AutoUpdate: Update download failed", e);
                await Dispatcher.InvokeAsync(() => {
                    MessageBox.Show("Update download failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }, _cancellationSource.Token);
                return;
            }

            await Dispatcher.InvokeAsync(() => {
                downloadState.Text = "Installing...";
                Owner.TaskbarItemInfo = new TaskbarItemInfo() {
                    ProgressState = TaskbarItemProgressState.Indeterminate
                };

                try {
                    UpdateInstaller.InstallUpdateAndRestart(downloadedUpdate);
                } catch (Exception e) {
                    Logger.LogException("AutoUpdate: Update install failed", e);
                    MessageBox.Show("Failed to install update!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }

                Owner.TaskbarItemInfo = new TaskbarItemInfo() {
                    ProgressValue = 0,
                    ProgressState = TaskbarItemProgressState.None
                };
            }, _cancellationSource.Token);
        }
    }
}
