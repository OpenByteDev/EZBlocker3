using System.Windows;

namespace EZBlocker3 {
    public partial class ErrorDialog : Window {
        private ErrorDialog() {
            InitializeComponent();

            dismissButton.Click += (_, __) => Close();
        }

        public static void Show(string message, Window owner) {
            if (owner.IsLoaded)
                _Show();
            else owner.Loaded += (_, __) => _Show();

            void _Show() {
                var dialog = new ErrorDialog();
                dialog.textBox.Text = message;
                dialog.Owner = owner;
                dialog.ShowDialog();
            }
        }
    }
}
