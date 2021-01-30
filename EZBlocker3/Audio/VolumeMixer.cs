using System;
using System.Diagnostics;

namespace EZBlocker3.Audio {
    public static class VolumeMixer {
        public static readonly string Path = Environment.GetEnvironmentVariable("WINDIR") + @"\System32\SndVol.exe";

        public static void Open() => Process.Start(Path);
    }
}
