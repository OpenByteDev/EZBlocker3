using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using EZBlocker3.Interop;

namespace EZBlocker3.Spotify {
    public static class SpotifyProcessUtils {
        public static IEnumerable<Process> GetSpotifyProcesses() {
            return Process.GetProcesses().Where(p => IsSpotifyProcess(p));
        }
        public static Process? GetMainSpotifyProcess() {
            return Array.Find(Process.GetProcesses(), p => IsMainWindowSpotifyProcess(p));
        }

        public static bool IsSpotifyProcess(Process? process) {
            if (process is null)
                return false;

            if (!process.ProcessName.StartsWith("spotify", StringComparison.OrdinalIgnoreCase))
                return false;

            return true;
        }

        public static bool IsMainWindowSpotifyProcess(Process? process) {
            if (!IsSpotifyProcess(process))
                return false;

            var mainWindowTitle = NativeUtils.GetMainWindowTitle(process!);

            if (string.IsNullOrWhiteSpace(mainWindowTitle))
                return false;

            if (mainWindowTitle == "G")
                return false; // dont ask me why a G window sometimes appears in the spotify process.

            return true;
        }
    }
}
