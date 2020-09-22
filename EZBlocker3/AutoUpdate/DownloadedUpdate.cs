using System;
using System.IO;

namespace EZBlocker3.AutoUpdate {
    public class DownloadedUpdate : IDisposable {

        public MemoryStream UpdateBytes { get; }

        public DownloadedUpdate(MemoryStream updateBytes) {
            UpdateBytes = updateBytes;
        }

        public void Dispose() {
            UpdateBytes.Dispose();
        }
    }
}
