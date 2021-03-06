﻿using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using EZBlocker3.AutoUpdate;
using EZBlocker3.Extensions;
using EZBlocker3.Logging;
using EZBlocker3.Settings;

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

            if (args.Length == 0) {
                Logger.LogInfo("Started without cli args");
            } else {
                Logger.LogInfo("Started with args: " + string.Join(" ", args));
            }

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
                Logger.LogInfo($"App exited with code {exitCode}");

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
                if (exception is not null) {
                    Logger.LogException("Unhandled exception", exception);
                } else {
                    Logger.LogError("Unhandled unknown exception");
                }
            } catch { }
        }

        private static async Task RunPipeServer(CancellationToken cancellationToken) {
            using var server = new NamedPipeServerStream(PipeName, PipeDirection.In, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

            cancellationToken.Register(() => {
                if (server.IsConnected)
                    server.Disconnect();
                server.Dispose();
            });
            cancellationToken.ThrowIfCancellationRequested();

            await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

            // we received a connection, which means another instance was started -> we bring the window to the front
            _ = Application.Current.Dispatcher.BeginInvoke(() => {
                Logger.LogInfo("App was started while already running -> bringing running window to front.");
                var mainWindow = (MainWindow)Application.Current.MainWindow;
                mainWindow.Dispatcher.BeginInvoke(() => mainWindow.Deminimize());
            });

            using var reader = new StreamReader(server);
            if (await reader.ReadLineAsync().ConfigureAwait(false) is string line && line == CliArgs.ProxyStartOption && !CliArgs.IsProxyStart) {
                StartWithSpotify.TransformToProxied();
            }

            server.Disconnect();

            // restart server
            await RunPipeServer(cancellationToken).ConfigureAwait(false);
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
            TaskScheduler.UnobservedTaskException += (s, e) =>
                Logger.LogException("Unobserved task exception: ", e.Exception);

            var exitCode = app.Run();

            if (CliArgs.IsProxyStart)
                StartWithSpotify.HandleProxiedExit();

            return exitCode;
        }
    }
}
