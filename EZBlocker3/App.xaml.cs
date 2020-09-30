using System;
using System.Windows;

namespace EZBlocker3 {
    public partial class App : Application {

        private static bool IsDebugBuild =
#if DEBUG 
            true;
#else
            false;
# endif
        public static bool ForceDebugMode = false;
        public static bool DebugModeEnabled => IsDebugBuild || ForceDebugMode || EZBlocker3.Properties.Settings.Default.DebugMode;

        protected override void OnActivated(EventArgs e) {
            base.OnActivated(e);

            if (EZBlocker3.Properties.Settings.Default.UpgradeRequired) {
                EZBlocker3.Properties.Settings.Default.Upgrade();
                EZBlocker3.Properties.Settings.Default.UpgradeRequired = false;
                EZBlocker3.Properties.Settings.Default.Save();
            }
        }

    }
}
