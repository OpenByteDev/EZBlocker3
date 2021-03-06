﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using EZBlocker3.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EZBlocker3.AutoUpdate {
    public static class UpdateChecker {
        private const string RELEASES_ENDPOINT = "https://api.github.com/repos/OpenByteDev/EZBlocker3/releases";

        private static Version GetCurrentVersion() {
            if (App.ForceUpdate)
                return new Version("0.0.0.0");
            else
                return App.Version;
        }

        public static async Task<UpdateInfo?> CheckForUpdate(CancellationToken cancellationToken = default) {
            Logger.AutoUpdate.LogDebug("Start update check");

            cancellationToken.ThrowIfCancellationRequested();

            var client = GlobalSingletons.HttpClient;
            var request = new HttpRequestMessage() {
                RequestUri = new Uri(RELEASES_ENDPOINT),
                Method = HttpMethod.Get
            };
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.Add("User-Agent", "EZBlocker3 Auto Updater");

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) {
                Logger.AutoUpdate.LogError($"Update check failed (Request failed with status code {response.StatusCode})");
                return null;
            }

            var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            using var reader = new JsonTextReader(new StreamReader(responseStream));

            var releases = await JToken.ReadFromAsync(reader, cancellationToken).ConfigureAwait(false);
            var latestReleaseInfo = releases?.FirstOrDefault(e => !e.Value<bool>("prerelease"));
            var latestVersionString = latestReleaseInfo?.Value<string>("tag_name");
            if (latestVersionString is null) {
                Logger.AutoUpdate.LogError("Update check failed (Failed to detect latest version)");
                return null;
            }

            var latestVersion = new Version(latestVersionString);
            var currentVersion = GetCurrentVersion();
            if (latestVersion <= currentVersion) {
                Logger.AutoUpdate.LogInfo($"Currently running latest version. ({latestVersion})");
                return null;
            }
            Logger.AutoUpdate.LogInfo($"Updated detected {currentVersion} -> {latestVersion}");

            var downloadUrl = latestReleaseInfo?["assets"]?
                .Where(e => e.Value<string>("content_type") == "application/x-msdownload")
                .Select(e => e.Value<string>("browser_download_url"))
                .FirstOrDefault();
            if (downloadUrl is null) {
                Logger.AutoUpdate.LogError("Update check failed (Failed to find download url)");
                return null;
            }
            return new UpdateInfo(downloadUrl, currentVersion, latestVersion);
        }
    }
}
