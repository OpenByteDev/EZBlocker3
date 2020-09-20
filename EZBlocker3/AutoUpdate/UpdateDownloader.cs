using EZBlocker3.Extensions;
using EZBlocker3.Logging;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace EZBlocker3.AutoUpdate {
    public class UpdateDownloader {

        public UpdateInfo Update { get; }

        public event DownloadProgressEventHandler? Progress;
        public delegate void DownloadProgressEventHandler(object sender, DownloadProgressEventArgs eventArgs);

        public UpdateDownloader(UpdateInfo update) {
            Update = update;
        }

        public Task<DownloadedUpdate> Run() => Run(CancellationToken.None);
        public async Task<DownloadedUpdate> Run(CancellationToken cancellationToken) {
            Logger.LogInfo("AutoUpdate: Start downloading update");

            // download file
            var client = GlobalSingletons.HttpClient;
            var response = await client.GetAsync(Update.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            Logger.LogDebug("AutoUpdate: Received response headers");

            using var contentStream = await response.Content.ReadAsStreamAsync();
            var contentLength = response.Content.Headers.ContentLength ?? long.MaxValue; // TODO

            // copy to memory stream
            var memoryStream = new MemoryStream();
            var progressHandler = new Progress<long>(totalBytes => {
                Progress?.Invoke(this, new DownloadProgressEventArgs(totalBytes, contentLength));
                Logger.LogDebug($"AutoUpdate: Received {totalBytes}/{contentLength} bytes");
            });
            await contentStream.CopyToAsync(memoryStream, progressHandler, cancellationToken);
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