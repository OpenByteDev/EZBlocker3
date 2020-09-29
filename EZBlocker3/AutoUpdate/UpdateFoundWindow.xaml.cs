using System.Windows;

namespace EZBlocker3.AutoUpdate {
    public partial class UpdateFoundWindow : Window {

        public enum UpdateDecision {
            Accept,
            NotNow,
            IgnoreUpdate
        }

        public UpdateDecision? Decision { get; private set; }

        public UpdateFoundWindow(UpdateInfo update) {
            InitializeComponent();

            versionInfoLabel.Text = $"{update.CurrentVersion} -> {update.UpdateVersion}";

            acceptDownloadButton.Click += (_, __) => {
                Close(UpdateDecision.Accept);
            };
            notNowButton.Click += (_, __) => {
                Close(UpdateDecision.NotNow);
            };
            ignoreUpdateButton.Click += (_, __) => {
                Close(UpdateDecision.IgnoreUpdate);
            };
        }

        private void Close(UpdateDecision decision) {
            Decision = decision;
            Close();
        }
    }
}
