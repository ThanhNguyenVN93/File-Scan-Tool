using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using frm_scan_online.Models;
using frm_scan_online.Providers.Http;
using frm_scan_online.Services;

namespace frm_scan_online.Providers
{
    // MetaDefender Cloud API v4 - https://www.opswat.com/docs/mdcloud/metadefender-cloud-api-v4
    public class MetaDefenderProvider : IScanProvider
    {
        private const string BaseUrl = "https://api.metadefender.com/v4/";
        // Couldn't find an official figure from OPSWAT's docs for the free tier - using ~140MB based
        // on common reference sources; the user should verify this themselves if uploading a
        // genuinely large file.
        private const long FileSizeLimit = 140L * 1024 * 1024;
        private const int PollIntervalMs = 5000;
        private const int MaxPollAttempts = 60; // up to ~5 minutes for a result

        public string Name => "MetaDefender";

        public long MaxFileSizeBytes => FileSizeLimit;

        private static string ApiKey => SettingsStore.Load().MetaDefenderApiKey;

        private static HttpClient CreateClient()
        {
            var apiKey = ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new MissingApiKeyException("MetaDefender");
            }

            var client = HttpClientFactory.Create(BaseUrl, TimeSpan.FromMinutes(5));
            client.DefaultRequestHeaders.Add("apikey", apiKey);
            return client;
        }

        public async Task<ScanResult> LookupByHashAsync(string sha256, CancellationToken ct)
        {
            using (var client = CreateClient())
            {
                var response = await client.GetAsync("hash/" + sha256, ct).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null; // no existing result on MetaDefender yet, the caller will upload the file
                }

                if (!response.IsSuccessStatusCode)
                {
                    return Error(await DescribeErrorAsync(response).ConfigureAwait(false));
                }

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return BuildScanResult(JObject.Parse(json));
            }
        }

        public async Task<ScanResult> UploadAndScanAsync(string filePath, IProgress<int> progress, CancellationToken ct)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return Error("File not found: " + filePath);
            }

            if (fileInfo.Length > FileSizeLimit)
            {
                return Error("File exceeds MetaDefender's estimated ~140MB limit (free tier).");
            }

            using (var client = CreateClient())
            {
                string dataId;
                progress?.Report(0);

                using (var fileStream = File.OpenRead(filePath))
                using (var content = new MultipartFormDataContent())
                using (var progressContent = new ProgressStreamContent(fileStream, uploaded =>
                {
                    var pct = (int)(uploaded * 30L / fileInfo.Length); // upload takes up 0-30%, the rest follows progress_percentage from MetaDefender
                    progress?.Report(Math.Min(pct, 30));
                }))
                {
                    progressContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(progressContent, "file", Path.GetFileName(filePath));

                    var uploadResponse = await client.PostAsync("file", content, ct).ConfigureAwait(false);
                    if (!uploadResponse.IsSuccessStatusCode)
                    {
                        return Error(await DescribeErrorAsync(uploadResponse).ConfigureAwait(false));
                    }

                    var uploadJson = await uploadResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    dataId = JObject.Parse(uploadJson)["data_id"]?.ToString();
                }

                progress?.Report(30);

                if (string.IsNullOrEmpty(dataId))
                {
                    return Error("MetaDefender did not return a data_id after uploading.");
                }

                for (var attempt = 0; attempt < MaxPollAttempts; attempt++)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(PollIntervalMs, ct).ConfigureAwait(false);

                    var pollResponse = await client.GetAsync("file/" + dataId, ct).ConfigureAwait(false);
                    if (!pollResponse.IsSuccessStatusCode)
                    {
                        return Error(await DescribeErrorAsync(pollResponse).ConfigureAwait(false));
                    }

                    var pollJson = await pollResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var root = JObject.Parse(pollJson);
                    var scanProgress = (int?)root["scan_results"]?["progress_percentage"] ?? 0;

                    progress?.Report(30 + (int)(scanProgress * 0.7));

                    if (scanProgress >= 100)
                    {
                        progress?.Report(100);
                        return BuildScanResult(root);
                    }
                }

                return Error("Timed out waiting for a result from MetaDefender, please try again later.");
            }
        }

        public async Task<QuotaInfo> GetQuotaAsync(CancellationToken ct)
        {
            // The JSON schema couldn't be verified against official documentation - this only guesses
            // at commonly-named fields (used/limit/allowed/consumed/total) at the top level or one
            // level deep in the response. If none are recognized, returns null (silently) - never
            // shows a wrong number.
            try
            {
                using (var client = CreateClient())
                {
                    var response = await client.GetAsync("apikey/limits/status", ct).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var root = JObject.Parse(json);

                    var quota = TryExtractQuota(root);
                    if (quota != null)
                    {
                        return quota;
                    }

                    var firstNestedObject = root.Properties()
                        .Select(p => p.Value as JObject)
                        .FirstOrDefault(v => v != null);

                    return TryExtractQuota(firstNestedObject);
                }
            }
            catch
            {
                return null;
            }
        }

        private static QuotaInfo TryExtractQuota(JObject obj)
        {
            if (obj == null)
            {
                return null;
            }

            var used = (int?)(obj["used"] ?? obj["consumed"]);
            var allowed = (int?)(obj["limit"] ?? obj["allowed"] ?? obj["total"]);

            if (used == null || allowed == null)
            {
                return null;
            }

            return new QuotaInfo { Used = used.Value, Allowed = allowed.Value, Label = "requests used (estimate)" };
        }

        public Task<ScanResult> ScanUrlAsync(string url, IProgress<int> progress, CancellationToken ct)
        {
            // MetaDefender has a GET /v4/url/{observable_url} endpoint per the official docs, but the
            // detailed JSON response schema couldn't be confirmed through research - guessing the scan
            // result structure wrong could show a false "clean" for a malicious URL, so this
            // deliberately does not implement it instead of guessing.
            return Task.FromResult(Error("MetaDefender URL scanning is not available in this tool yet " +
                "(the API response format hasn't been verified against a real key)."));
        }

        private ScanResult BuildScanResult(JObject root)
        {
            var scanResults = root["scan_results"];
            var fileInfo = root["file_info"];

            int totalAvs = (int?)scanResults?["total_avs"] ?? 0;
            int totalDetected = (int?)scanResults?["total_detected_avs"] ?? 0;

            var engineResults = new Dictionary<string, string>();
            if (scanResults?["scan_details"] is JObject scanDetails)
            {
                foreach (var prop in scanDetails.Properties())
                {
                    var resultCode = (int?)prop.Value["scan_result_i"] ?? 0;
                    var threatFound = prop.Value["threat_found"]?.ToString();
                    if (resultCode != 0 || !string.IsNullOrEmpty(threatFound))
                    {
                        engineResults[prop.Name] = string.IsNullOrEmpty(threatFound) ? "Detected (code " + resultCode + ")" : threatFound;
                    }
                }
            }

            return new ScanResult
            {
                SourceName = Name,
                IsClean = totalDetected == 0,
                DetectionRatio = $"{totalDetected}/{totalAvs}",
                EngineResults = engineResults,
                Sha256 = fileInfo?["sha256"]?.ToString(),
                Md5 = fileInfo?["md5"]?.ToString(),
                ErrorMessage = null
            };
        }

        private ScanResult Error(string message)
        {
            return new ScanResult { SourceName = Name, ErrorMessage = message };
        }

        private async Task<string> DescribeErrorAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized || response.StatusCode == HttpStatusCode.Forbidden)
            {
                return "MetaDefender API key is invalid or lacks access.";
            }

            if ((int)response.StatusCode == 429)
            {
                return "MetaDefender quota/rate limit exceeded, please try again later.";
            }

            string body = null;
            try
            {
                body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            catch
            {
                // ignore, fall back to the default message below
            }

            string detail = null;
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    detail = JObject.Parse(body)["error"]?.ToString();
                }
                catch
                {
                    // body isn't valid JSON, ignore
                }
            }

            return detail ?? $"MetaDefender HTTP error {(int)response.StatusCode}: {response.ReasonPhrase}";
        }
    }
}
