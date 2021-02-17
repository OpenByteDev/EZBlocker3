using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Lazy;

namespace EZBlocker3.Logging {
    internal static class Logger {
        [Lazy]
        private static string LogFilePath => Path.Combine(App.Directory, "log.txt");

        private static readonly object _lock_logFile = new();

        public static void Log(LogLevel level, string message, string? area = null) {
            if (App.DebugModeEnabled) {
                if (area is null) {
                    area = string.Empty;
                } else {
                    area = $" ({area})";
                }

                var paddedLevel = level.ToString().PadRight(nameof(LogLevel.Information).Length);

                var formatted = $"[{DateTime.Now}] {paddedLevel}{area} {message}";
                Trace.WriteLine(formatted);

                lock (_lock_logFile) {
                    using var writer = new StreamWriter(LogFilePath, append: true);
                    writer.WriteLine(formatted);
                }
            }
        }
        public static void Log(LogLevel level, object message, string? area = null) => Log(level, message.ToString(), area);
        public static void LogDebug(object message, string? area = null) => Log(LogLevel.Debug, message, area);
        public static void LogInfo(object message, string? area = null) => Log(LogLevel.Information, message, area);
        public static void LogWarning(object message, string? area = null) => Log(LogLevel.Warning, message, area);
        public static void LogError(object message, string? area = null) => Log(LogLevel.Error, message, area);
        public static void LogLastWin32Error(string? area = null) => LogError(new Win32Exception().Message, area);
        public static void LogException(string message, Exception exception, string? area = null) => LogError(message + "\n" + exception.ToString(), area);

        public static NamedLogger GetNamed(string name) => new(name);

        public static NamedLogger AutoUpdate = GetNamed("AutoUpdate");
        public static NamedLogger Hook = GetNamed("SpotifyHook");
        public static NamedLogger AdSkipper = GetNamed("Ad Skipper");
    }
}
