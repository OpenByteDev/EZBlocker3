namespace EZBlocker3.AutoUpdate {
    public readonly struct UpdateInfo {

        public readonly string DownloadUrl;

        public UpdateInfo(string downloadUrl) {
            DownloadUrl = downloadUrl;
        }
    }
}