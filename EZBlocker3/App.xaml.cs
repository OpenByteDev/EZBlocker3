using System;
using System.Windows;

namespace EZBlocker3 {
    public partial class App : Application {

        public static bool DebugModeEnabled
            #if DEBUG
                        = true;
            #else
                        = false;
            #endif

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);

            if (EZBlocker3.Properties.Settings.Default.DebugMode)
                DebugModeEnabled = true;
        }

    }
}
