using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace EZBlocker3.Extensions {
    internal static class NamedPipeServerStreamExtensions {

        // public static Task WaitForConnectionAsync(this NamedPipeServerStream serverStream, CancellationToken cancellationToken) {
        //     return Task.Factory.FromAsync(serverStream.BeginWaitForConnection, serverStream.EndWaitForConnection, null);
        // }

    }
}
