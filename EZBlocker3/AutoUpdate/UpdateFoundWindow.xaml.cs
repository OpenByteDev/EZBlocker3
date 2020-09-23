using System.Windows;

namespace EZBlocker3.AutoUpdate {
    public partial class UpdateFoundWindow : Window {

        public UpdateFoundWindow(UpdateInfo update) {
            InitializeComponent();

            versionInfoLabel.Text = $"{update.CurrentVersion} -> {update.UpdateVersion}";

            acceptDownloadButton.Click += (_, __) => {
                var downloadUpdateWindow = new DownloadUpdateWindow(update);
                Hide();
                downloadUpdateWindow.ShowDialog();
                Close();
            };
            notNowButton.Click += (_, __) => {
                Close();
            };
            ignoreUpdateButton.Click += (_, __) => {
                Close();
                // TODO: remmeber to not ask again for this version.
            };
        }
    }
}
