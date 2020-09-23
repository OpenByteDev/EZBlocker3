using System;

namespace EZBlocker3.AutoUpdate {
    public record UpdateInfo(string DownloadUrl, Version CurrentVersion, Version UpdateVersion);
}
