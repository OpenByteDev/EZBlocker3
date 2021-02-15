using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using EZBlocker3.Extensions;
using EZBlocker3.Logging;

namespace EZBlocker3.AutoUpdate {
    public class UpdateDownloader {
        public event EventHandler<DownloadProgressEventArgs>? Progress;

        public async Task<DownloadedUpdate> Download(UpdateInfo update, CancellationToken cancellationToken = default) {
            Logger.AutoUpdate.LogInfo("Start downloading update");

            cancellationToken.ThrowIfCancellationRequested();

            // download file
            var client = GlobalSingletons.HttpClient;
            var response = await client.GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            Logger.AutoUpdate.LogDebug("Received response headers");

            using var contentStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var contentLength = response.Content.Headers.ContentLength;

            // copy to memory stream
            var memoryStream = new MemoryStream();
            if (contentLength is long totalBytes) {
                var progressHandler = new Progress<long>(bytesReceived => {
                    Progress?.Invoke(this, new DownloadProgressEventArgs(bytesReceived, totalBytes));
                    Logger.AutoUpdate.LogDebug($"Received {bytesReceived}/{totalBytes} bytes");
                });
                await contentStream.CopyToAsync(memoryStream, progressHandler, cancellationToken).ConfigureAwait(false);
            } else {
                Logger.AutoUpdate.LogWarning("Failed to determine response content length.");
                await contentStream.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            Logger.AutoUpdate.LogInfo("Completed update download");

            return new DownloadedUpdate(memoryStream);
        }
    }

    public class DownloadProgressEventArgs : EventArgs {
        public float DownloadPercentage => (float)BytesReceived / TotalBytesToReceive;
        public long BytesReceived { get; }
        public long TotalBytesToReceive { get; }

        public DownloadProgressEventArgs(long bytesReceived, long totalBytesToReceive) {
            BytesReceived = bytesReceived;
            TotalBytesToReceive = totalBytesToReceive;
        }
    }
}