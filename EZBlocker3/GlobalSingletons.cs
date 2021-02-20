using System.Net.Http;

namespace EZBlocker3 {
    public static class GlobalSingletons {
        private static HttpClient? _httpClient;

        public static HttpClient HttpClient => _httpClient ??= new();

        public static void Dispose() {
            _httpClient?.Dispose();
        }
    }
}
