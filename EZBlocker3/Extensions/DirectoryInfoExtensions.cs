using System.IO;

namespace EZBlocker3.Extensions {
    internal static class DirectoryInfoExtensions {

        public static void RecursiveDelete(this DirectoryInfo directory) {
            if (!directory.Exists)
                return;

            foreach (var subDirectory in directory.EnumerateDirectories())
                RecursiveDelete(subDirectory);

            directory.Delete(recursive: true);
        }

    }
}
