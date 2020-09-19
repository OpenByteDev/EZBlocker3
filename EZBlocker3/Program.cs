using System;
using System.IO.Pipes;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace EZBlocker3 {
    internal static class Program {

        public static readonly string AppName = Assembly.GetEntryAssembly().GetName().Name;
        public static readonly string SingletonMutexName = AppName + "_SingletonMutex";
        public static readonly string PipeName = AppName + "_IPC";

        [STAThread]
        public static int Main() {
            using var mutex = new Mutex(true, SingletonMutexName, out var notAlreadyRunning);
            if (notAlreadyRunning) { // we are the only one around :(
                using var server = new NamedPipeServerStream(PipeName, PipeDirection.Out, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                server.BeginWaitForConnection(ConnectionHandler, server);
                var exitCode = RunApp();

                server.Close();
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
                var mainWindow = Application.Current.MainWindow;
                mainWindow.Dispatcher.Invoke(() => {
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                });
            });
        }
        
        private static int RunApp() {
            var app = new App();
            app.InitializeComponent();
            return app.Run();
        }
    }

}
