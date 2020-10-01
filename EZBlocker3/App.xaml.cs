using System;
using System.Reflection;
using System.Windows;

namespace EZBlocker3 {
    public partial class App : Application {

        private static bool IsDebugBuild =
        public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
        public static readonly AssemblyName AssemblyName = Assembly.GetName();
        public static readonly string Name = AssemblyName.Name;
        public static readonly string ProductName = Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public static readonly string Location = Assembly.Location;
        public static readonly Version Version = AssemblyName.Version;

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
