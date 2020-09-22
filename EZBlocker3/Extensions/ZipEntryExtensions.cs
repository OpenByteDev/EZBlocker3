using Ionic.Zip;
using System.IO;

namespace EZBlocker3.Extensions {
    internal static class ZipEntryExtensions {

        public static void ExtractTo(this ZipEntry entry, string path) {
            using var file = File.OpenWrite(path);
            entry.Extract(file);
        }

    }
}
