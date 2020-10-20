using EZBlocker3.Logging;
using EZBlocker3.Settings;
using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace EZBlocker3 {
    public partial class App : Application {

        public static readonly Assembly Assembly = Assembly.GetExecutingAssembly();
        public static readonly AssemblyName AssemblyName = Assembly.GetName();
        public static readonly string Name = AssemblyName.Name;
        public static readonly string ProductName = Assembly.GetCustomAttribute<AssemblyProductAttribute>().Product;
        public static readonly string CompanyName = Assembly.GetCustomAttribute<AssemblyCompanyAttribute>().Company;
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
        public static bool ForceUpdate = false;
        public static bool ForceUpdateCheck = IsDebugBuild || ForceUpdate;
        public static bool SaveSettingsOnClose = true;

        protected override void OnStartup(StartupEventArgs eventArgs) {
            base.OnStartup(eventArgs);

            // enable all security protocols
            // without this statement https requests fail.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            // reenable settings after update (disabled in UpdateInstaller.InstallUpdateAndRestart)
            var settings = EZBlocker3.Properties.Settings.Default;
            if (settings.UpgradeRequired || Program.CliArgs.IsUpdateRestart) {
                if (StartWithSpotify.Available)
                    StartWithSpotify.SetEnabled(settings.StartWithSpotify);
                Autostart.SetEnabled(settings.StartOnLogin);
            }

            // upgrade settings on first start (after update)
            if (settings.UpgradeRequired) {
                settings.Upgrade();
                settings.UpgradeRequired = false;
                settings.Save();
            }

            // check if executable has moved
            if (App.Location != settings.AppPath) {
                try {
                    if (settings.StartOnLogin)
                        Autostart.SetEnabled(settings.StartOnLogin);
                    // StartWithSpotify.SetEnabled(settings.StartWithSpotify);
                } catch (Exception e) {
                    Logger.LogException("Failed to adjust to changed app path:", e);
                }

                settings.AppPath = App.Location;
            }

            if (settings.StartWithSpotify) {
                Task.Run(static () => {
                    // Ensure that the proxy is still installed correctly if enabled.
                    StartWithSpotify.Enable();

                    // start spotify if start with spotify is enabled but we did not start through the proxy
                    if (!Program.CliArgs.IsProxyStart) {
                        StartWithSpotify.TransformToProxied();
                        StartWithSpotify.StartSpotify();
                    }
                });
            }
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);

            GlobalSingletons.Dispose();
        }

    }
}
