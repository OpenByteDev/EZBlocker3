using EZBlocker3.Extensions;
using EZBlocker3.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EZBlocker3.AutoUpdate {
    public class UpdateDownloader {

        public event DownloadProgressEventHandler? Progress;
        public delegate void DownloadProgressEventHandler(object sender, DownloadProgressEventArgs eventArgs);

        public Task<DownloadedUpdate> Download(UpdateInfo update) => Download(update, CancellationToken.None);
        public async Task<DownloadedUpdate> Download(UpdateInfo update, CancellationToken cancellationToken) {
            Logger.LogInfo("AutoUpdate: Start downloading update");

            // download file
            var client = GlobalSingletons.HttpClient;
            var response = await client.GetAsync(update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            Logger.LogDebug("AutoUpdate: Received response headers");

            using var contentStream = await response.Content.ReadAsStreamAsync();
            var contentLength = response.Content.Headers.ContentLength;

            // copy to memory stream
            var memoryStream = new MemoryStream();
            if (contentLength is long totalBytes) {
                var progressHandler = new Progress<long>(bytesReceived => {
                    Progress?.Invoke(this, new DownloadProgressEventArgs(bytesReceived, totalBytes));
                    Logger.LogDebug($"AutoUpdate: Received {bytesReceived}/{totalBytes} bytes");
                });
                await contentStream.CopyToAsync(memoryStream, progressHandler, cancellationToken);
            } else {
                Logger.LogWarning($"AutoUpdate: Failed to determine response content length.");
                await contentStream.CopyToAsync(memoryStream, cancellationToken);
            }
            memoryStream.Seek(0, SeekOrigin.Begin);

            Logger.LogInfo("AutoUpdate: Completed update download");

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