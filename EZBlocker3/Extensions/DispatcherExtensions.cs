using System;
using System.Threading;
using System.Windows.Threading;

namespace EZBlocker3.Extensions {
    internal static class DispatcherExtensions {

        public static DispatcherOperation InvokeAsync(this Dispatcher dispatcher, Action callback, CancellationToken cancellationToken) =>
            dispatcher.InvokeAsync(callback, DispatcherPriority.Normal, cancellationToken);

        public static DispatcherOperation<T> InvokeAsync<T>(this Dispatcher dispatcher, Func<T> callback, CancellationToken cancellationToken) =>
            dispatcher.InvokeAsync(callback, DispatcherPriority.Normal, cancellationToken);

        public static DispatcherOperation BeginInvoke(this Dispatcher dispatcher, Action callback) =>
            dispatcher.BeginInvoke(DispatcherPriority.Normal, callback);

        public static void BeginInvokeShutdown(this Dispatcher dispatcher) =>
            dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);

    }
}
