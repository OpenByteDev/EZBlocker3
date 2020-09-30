using System;
using System.Windows;

namespace EZBlocker3.Utils {
    internal static class WindowHelper {

        public static void ApplySizeToContentFix(Window window) {
            void Handler(object sender, EventArgs eventArgs) {
                window.InvalidateMeasure();
                window.SourceInitialized -= Handler;
            }

            window.SourceInitialized += Handler;
        }

        public static readonly DependencyProperty ApplySizeToContentFixProperty =
            DependencyProperty.RegisterAttached(
                "ApplySizeToContentFix",
                typeof(bool),
                typeof(WindowHelper),
                new PropertyMetadata(OnApplySizeToContentFixChanged));

        public static bool GetApplySizeToContentFix(Window window) {
            return (bool)window.GetValue(ApplySizeToContentFixProperty);
        }

        public static void SetApplySizeToContentFix(Window window, bool value) {
            window.SetValue(ApplySizeToContentFixProperty, value);
        }

        private static void OnApplySizeToContentFixChanged(DependencyObject obj, DependencyPropertyChangedEventArgs eventArgs) {
            bool newValue = (bool)eventArgs.NewValue;

            if (newValue && obj is Window window)
                ApplySizeToContentFix(window);
        }

    }
}
