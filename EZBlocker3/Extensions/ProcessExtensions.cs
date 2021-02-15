using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Lazy;

namespace EZBlocker3.Extensions {
    internal static class ProcessExtensions {
        [Lazy]
        private static Func<Process, bool> _getProcessAssociatedFunc {
            get {
                // Expression Trees let us change a private field and are faster than reflection (if called multiple times)
                var processParamter = Expression.Parameter(typeof(Process), "process");
                var associatedProperty = Expression.Property(processParamter, "Associated");
                // var returnStatement = Expression.Return()
                // var returnLabel = Expression.Label();
                var lambda = Expression.Lambda<Func<Process, bool>>(associatedProperty, processParamter);
                return lambda.Compile();
            }
        }
        public static bool IsAssociated(this Process process) {
            return _getProcessAssociatedFunc(process);
        }

        public static async Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default) {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            try {
                if (process.HasExited)
                    return;

                process.EnableRaisingEvents = true;
                process.Exited += Process_Exited;

                using var registration = cancellationToken.Register(() => tcs.TrySetCanceled(), false);
                await tcs.Task.ConfigureAwait(false);
            } finally {
                process.Exited -= Process_Exited;
            }

            void Process_Exited(object sender, EventArgs e) {
                tcs.TrySetResult(true);
            }
        }

        public static Task WaitForExitAsync(this Process process, TimeSpan timeout, CancellationToken cancellationToken = default) {
            return Task.WhenAny(process.WaitForExitAsync(cancellationToken), Task.Delay(timeout, cancellationToken)).Unwrap();
        }
    }
}
