using System;

namespace EZBlocker3.Logging {
    internal sealed class NamedLogger {
        private readonly string name;

        public NamedLogger(string name) {
            this.name = name;
        }

        public void Log(LogLevel level, string message) => Logger.Log(level, message, name);
        public void Log(LogLevel level, object message) => Logger.Log(level, message, name);
        public void LogDebug(object message) => Logger.LogDebug(message, name);
        public void LogInfo(object message) => Logger.LogInfo(message, name);
        public void LogWarning(object message) => Logger.LogWarning(message, name);
        public void LogError(object message) => Logger.LogError(message, name);
        public void LogLastWin32Error() => Logger.LogLastWin32Error(name);
        public void LogException(string message, Exception exception) => Logger.LogException(message, exception, name);
    }
}