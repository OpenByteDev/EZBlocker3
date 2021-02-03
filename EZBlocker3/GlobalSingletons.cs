using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace EZBlocker3 {
    public static class GlobalSingletons {
        private static HttpClient? _httpClient;

        [SuppressMessage("Performance", "U2U1025:Avoid instantiating HttpClient", Justification = "HttpClient is only instantiated once.")]
        public static HttpClient HttpClient => _httpClient ??= new HttpClient();

        public static void Dispose() {
            _httpClient?.Dispose();
        }
    }
}
