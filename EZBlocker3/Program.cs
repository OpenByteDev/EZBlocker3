using EZBlocker3.AutoUpdate;
using EZBlocker3.Logging;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows;

namespace EZBlocker3 {
    internal static class Program {

        private static readonly string SingletonMutexName = App.Name + "_SingletonMutex";
        private static readonly string PipeName = App.Name + "_IPC";

#pragma warning disable CS8618
        public static CliArgs CliArgs;
#pragma warning restore CS8618

        [STAThread]
        public static int Main(string[] args) {
            CliArgs = CliArgs.Parse(args);

            using var mutex = new Mutex(initiallyOwned: true, SingletonMutexName, out var notAlreadyRunning);

            if (CliArgs.IsUpdateRestart) {
                try {
                    // wait for old version to exit and release the mutex.
                    mutex.WaitOne(TimeSpan.FromSeconds(5), exitContext: false);
                } catch (Exception e) {
                    Debugger.Launch();
                    Logger.LogException("Restart failed after update", e);
                }

                // the application has exited so we are not already running.
                notAlreadyRunning = true;
            }

            if (notAlreadyRunning) { // we are the only one around :(
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.Out, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                server.BeginWaitForConnection(ConnectionHandler, server);

                if (CliArgs.ForceDebugMode)
                    App.ForceDebugMode = true;

                var exitCode = RunApp();

                if (server.IsConnected)
                    server.Disconnect();
                server.Close();
                mutex.ReleaseMutex();
                return exitCode;
            } else { // another instance is already running 
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.In, PipeOptions.Asynchronous);
                client.Connect(10000);
                client.Close();
                return 0;
            }
        }
        private static void ConnectionHandler(IAsyncResult result) {
            if (!(result.AsyncState is NamedPipeServerStream server))
                throw new IllegalStateException();

            server?.EndWaitForConnection(result);
            server?.Disconnect();
            server?.BeginWaitForConnection(ConnectionHandler, server);

            Application.Current.Dispatcher.Invoke(() => {
                var mainWindow = (MainWindow) Application.Current.MainWindow;
                mainWindow.Dispatcher.Invoke(() => {
                    mainWindow.Deminimize();
                });
            });
        }

        private static int RunApp() {
            // enable all protocols
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

            var app = new App();
            app.InitializeComponent();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;

            return app.Run();
        }
    }

}
