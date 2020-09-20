﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace EZBlocker3.Logging {
    public static class Logger {

        public static bool EnableLogFile
#if DEBUG
            = true;
#else
            = false;
#endif
        private static object _lock_logFile = new object();

        public static void Log(LogLevel level, string message) {
            var formatted = $"[{DateTime.Now}][{level}] {message}";
            Trace.WriteLine(formatted);

            if (EnableLogFile) {
                lock (_lock_logFile) {
                    var directory = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                    var logFilePath = Path.Combine(directory, "log.txt");
                    using var writer = new StreamWriter(logFilePath, append: true);
                    writer.WriteLine(formatted);
                }
            }
        }
        public static void Log(LogLevel level, object message) => Log(level, message.ToString());
        public static void LogDebug(object message) => Log(LogLevel.Debug, message);
        public static void LogInfo(object message) => Log(LogLevel.Information, message);
        public static void LogWarning(object message) => Log(LogLevel.Warning, message);
        public static void LogError(object message) => Log(LogLevel.Error, message);
        public static void LogLastWin32Error() {
            var errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            LogError(errorMessage);
        }

    }
}
