using EZBlocker3.AutoUpdate;
using EZBlocker3.Logging;
using EZBlocker3.Extensions;
using EZBlocker3.Settings;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
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
            AppDomain.CurrentDomain.UnhandledException += (s, e) => OnUnhandledException(e.ExceptionObject as Exception);

            CliArgs = CliArgs.Parse(args);

            using var mutex = new Mutex(initiallyOwned: true, SingletonMutexName, out var notAlreadyRunning);

            if (CliArgs.IsUpdateRestart) {
                try {
                    // wait for old version to exit and release the mutex.
                    mutex.WaitOne(TimeSpan.FromSeconds(10), exitContext: false);
                    UpdateInstaller.CleanupUpdate();
                } catch (Exception e) {
                    Logger.LogException("Restart failed after update", e);
                }

                // the application has exited so we are not already running.
                notAlreadyRunning = true;
            }

            if (notAlreadyRunning) { // we are the only one around :(
                var cancellationTokenSource = new CancellationTokenSource();
                Task.Run(() => RunPipeServer(cancellationTokenSource.Token), cancellationTokenSource.Token);

                var exitCode = RunApp();

                cancellationTokenSource.Cancel();
                mutex.ReleaseMutex();
                return exitCode;
            } else { // another instance is already running 
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous);
                client.Connect(TimeSpan.FromSeconds(10).Milliseconds);

                if (CliArgs.IsProxyStart) {
                    using var writer = new StreamWriter(client);
                    writer.WriteLine(CliArgs.ProxyStartOption);
                    writer.Flush();
                }

                return 0;
            }
        }

        private static void OnUnhandledException(Exception? exception) {
            try {
                Logger.LogError("Unhandled exception:\n" + exception);
            } catch { }
        }

        private static async Task RunPipeServer(CancellationToken cancellationToken) {
            using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            cancellationToken.Register(() => {
                if (server.IsConnected)
                    server.Disconnect();
                server.Dispose();
            });
            await server.WaitForConnectionAsync(cancellationToken);

            // we received a connection, which means another instance was started -> we bring the window to the front
            _ = Application.Current.Dispatcher.BeginInvoke(() => {
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.Dispatcher.BeginInvoke(() => {
                    mainWindow.Deminimize();
                });
            });

            using var reader = new StreamReader(server);
            if (await reader.ReadLineAsync() is string line) {
                if (line == CliArgs.ProxyStartOption && !CliArgs.IsProxyStart) {
                    StartWithSpotify.TransformToProxied();
                }
            }

            server.Disconnect();

            // restart server
            await RunPipeServer(cancellationToken);
        }

        private static int RunApp() {
            if (CliArgs.ForceDebugMode)
                App.ForceDebugMode = true;

            if (CliArgs.IsProxyStart)
                StartWithSpotify.HandleProxiedStart();

            var app = new App();
            app.InitializeComponent();
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;
            app.DispatcherUnhandledException += (s, e) => OnUnhandledException(e.Exception);
            TaskScheduler.UnobservedTaskException += (s, e) => {
                Logger.LogException("Unobserved task exception: ", e.Exception);
            };

            var exitCode = app.Run();

            if (CliArgs.IsProxyStart)
                StartWithSpotify.HandleProxiedExit();

            return exitCode;
        }
    }
}
