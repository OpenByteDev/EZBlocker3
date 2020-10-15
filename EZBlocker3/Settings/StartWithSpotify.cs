using EZBlocker3.Logging;
using EZBlocker3.Utils;
using Lazy;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace EZBlocker3.Settings {
    public static class StartWithSpotify {

        [Lazy]
        private static string SpotifyPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\Spotify\Spotify.exe";
        [Lazy]
        private static string RealSpotifyPath => Path.ChangeExtension(SpotifyPath, "real.exe");
        [Lazy]
        private static string ProxyTempPath => Path.ChangeExtension(SpotifyPath, "proxy.exe");

        public static void SetEnabled(bool enabled) {
            if (IsProxyInstalled()) {
                if (!enabled)
                    Disable();
            } else {
                if (enabled)
                    Enable();
            }
        }
        public static void Enable() => InstallProxy();
        public static void Disable() => UninstallProxy();

        public static bool IsProxyInstalled() {
            if (!File.Exists(RealSpotifyPath))
                return false;
            return !IsInvalidStateAfterSpotifyUpdate();
        }
        public static void InstallProxy() {
            HandleInvalidStateAfterUpdate();

            var tempIconFilePath = Path.ChangeExtension(SpotifyPath, ".ico.temp");
            try {
                using var spotifyIcon = Icon.ExtractAssociatedIcon(SpotifyPath);
                using var spotifyIconBitmap = spotifyIcon.ToBitmap();
                BitmapUtils.SaveAsIcon(spotifyIconBitmap, tempIconFilePath);

                File.Delete(ProxyTempPath);
                if (GenerateProxy(ProxyTempPath, tempIconFilePath)) {
                    Logger.LogInfo("Settings: Successfully generated proxy executable");
                    File.Move(SpotifyPath, RealSpotifyPath);
                    File.Move(ProxyTempPath, SpotifyPath);
                } else {
                    Logger.LogError("Settings: Failed to generate proxy executable");
                }
            } catch {
                File.Delete(ProxyTempPath);
                throw;
            } finally {
                File.Delete(tempIconFilePath);
            }
        }
        public static void UninstallProxy() {
            HandleInvalidStateAfterUpdate();

            if (File.Exists(RealSpotifyPath)) {
                // spotify is not running
                File.Delete(SpotifyPath);
                File.Move(RealSpotifyPath, SpotifyPath);
            } else {
                // spotify is running
                File.Delete(ProxyTempPath);
            }
        }

        private static bool IsInvalidStateAfterSpotifyUpdate() {
            // check if executable is smaller than 5MB
            // the real spotify executable is > 20MB and the proxy should be less than 1MB
            // if the size is > 5MB that means that spotify was updated and replaced the proxy
            return new FileInfo(SpotifyPath).Length > 1024 * 1024 * 5;
        }
        private static void HandleInvalidStateAfterUpdate() {
            if (!IsInvalidStateAfterSpotifyUpdate())
                return;

            try {
                File.Delete(RealSpotifyPath);
            } catch {
                File.Move(RealSpotifyPath, Path.GetTempFileName());
            }
        }

        public static bool GenerateProxy(string executablePath, string iconPath) {
            var parameters = new CompilerParameters {
                GenerateExecutable = true,
                OutputAssembly = executablePath,
                GenerateInMemory = false
            };
            parameters.ReferencedAssemblies.Add("System.dll");
            parameters.CompilerOptions = $"/target:winexe \"/win32icon:{iconPath}\"";

            var provider = CodeDomProvider.CreateProvider("CSharp");
            var code = GetProxyCode(App.Location, appArgs: CliArgs.ProxyStartOption, RealSpotifyPath, spotifyArgs: string.Empty);
            var result = provider.CompileAssemblyFromSource(parameters, code);

            foreach (CompilerError error in result.Errors)
                Logger.LogWarning($"Settings: Redirection executable generation {(error.IsWarning ? "warning" : "error")}:\n{error.ErrorText}");

            return result.Errors.Count == 0;
        }

        public static void HandleProxiedStart() {
            Logger.LogInfo("Started through proxy executable");

            if (!File.Exists(RealSpotifyPath)) {
                Logger.LogWarning("Started through proxy executable when no proxy is present");
                return;
            }

            try {
                File.Delete(ProxyTempPath);
                File.Move(SpotifyPath, ProxyTempPath);
                File.Move(RealSpotifyPath, SpotifyPath);
            } catch (Exception e) {
                Logger.LogException("Failed to handle proxied start:", e);
            }
        }
        public static void HandleProxiedExit() {
            Logger.LogInfo("Reset proxy executable");

            if (!File.Exists(ProxyTempPath)) {
                Logger.LogWarning("Failed to reset proxy as no proxy is present");
                return;
            }

            try {
                File.Move(SpotifyPath, RealSpotifyPath);
                File.Move(ProxyTempPath, SpotifyPath);
            } catch (Exception e) {
                Logger.LogException("Failed to handle proxied exit:", e);
            }
        }
        }

        private static string GetProxyCode(string appPath, string appArgs, string spotifyPath, string spotifyArgs) => @"
using System.Diagnostics;

public static class Proxy {
    public static void Main() {
        var appPath = @""" + appPath + @""";
        var appArgs = @""" + appArgs + @""";
        var spotifyPath = @""" + spotifyPath + @""";
        var spotifyArgs = @""" + spotifyArgs + @""";

        Process.Start(spotifyPath, spotifyArgs).Dispose();
        Process.Start(appPath, appArgs).Dispose();
    }
}";
    }
}

