using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EZBlocker3.Extensions {
    internal static class StreamExtensions {

        // https://referencesource.microsoft.com/#mscorlib/system/io/stream.cs,2a0f078c2e0c0aa8
        private const int DefaultCopyBufferSize = 81920;

        public static Task CopyToAsync(this Stream source, Stream destination, CancellationToken cancellationToken) =>
            source.CopyToAsync(destination, DefaultCopyBufferSize, cancellationToken);
        public static Task CopyToAsync(this Stream source, Stream destination, IProgress<long>? progress = null, CancellationToken cancellationToken = default) =>
            CopyToAsync(source, destination, DefaultCopyBufferSize, progress, cancellationToken);
        public static async Task CopyToAsync(this Stream source, Stream destination, int bufferSize, IProgress<long>? progress = null, CancellationToken cancellationToken = default) {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (progress == null) {
                await source.CopyToAsync(destination, bufferSize, cancellationToken);
                return;
            }

            // from https://referencesource.microsoft.com/#mscorlib/system/io/unmanagedmemorystreamwrapper.cs,05bf6506f3abc6ed
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (bufferSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Positive number required."); // Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum")
            if (!source.CanRead && !source.CanWrite)
                throw new ObjectDisposedException(null, "Cannot access a closed stream."); // Environment.GetResourceString("ObjectDisposed_StreamClosed")
            if (!destination.CanRead && !destination.CanWrite)
                throw new ObjectDisposedException(nameof(destination), "Cannot access a closed stream."); // Environment.GetResourceString("ObjectDisposed_StreamClosed")
            if (!source.CanRead)
                throw new NotSupportedException("Stream does not support reading."); // Environment.GetResourceString("NotSupported_UnreadableStream")
            if (!destination.CanWrite)
                throw new NotSupportedException("Stream does not support writing."); // Environment.GetResourceString("NotSupported_UnwritableStream");

            var buffer = new byte[bufferSize];
            long totalBytesRead = 0;
            int bytesRead;
            while ((bytesRead = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false)) != 0) {
                await destination.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
            }
        }
    }
}
