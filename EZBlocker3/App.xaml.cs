using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace EZBlocker3 {
    public partial class App : Application {

        public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
        public static readonly AssemblyName AssemblyName = Assembly.GetName();
        public static readonly string Name = AssemblyName.Name;
        public static readonly string ProductName = Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public static readonly string Location = Assembly.Location;
        public static readonly string Directory = Path.GetDirectoryName(Location);
        public static readonly Version Version = AssemblyName.Version;

        private const bool IsDebugBuild =
#if DEBUG 
            true;
#else
            false;
# endif
        public static bool ForceDebugMode = false;
        public static bool DebugModeEnabled => IsDebugBuild || ForceDebugMode || EZBlocker3.Properties.Settings.Default.DebugMode;

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            if (EZBlocker3.Properties.Settings.Default.UpgradeRequired) {
                EZBlocker3.Properties.Settings.Default.Upgrade();
                EZBlocker3.Properties.Settings.Default.UpgradeRequired = false;
                EZBlocker3.Properties.Settings.Default.Save();
            if (App.Location != settings.AppPath) {
                try {
                    Autostart.SetEnabled(settings.StartOnLogin);
                    StartWithSpotify.SetEnabled(settings.StartWithSpotify);
                } catch (Exception e) {
                    Logger.LogException("Failed to adjust to changed app path:", e);
                }

                settings.AppPath = App.Location;
            }
        }

    }
}
