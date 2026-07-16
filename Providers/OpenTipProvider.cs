using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;
using frm_scan_online.Models;
using frm_scan_online.Providers.Http;
using frm_scan_online.Services;

namespace frm_scan_online.Providers
{
    // Kaspersky OpenTIP API v1 - https://opentip.kaspersky.com/Help/Doc_data/WorkingWithAPI.htm
    public class OpenTipProvider : IScanProvider
    {
        private const string BaseUrl = "https://opentip.kaspersky.com/api/v1/";
        private const long FileSizeLimit = 256L * 1024 * 1024; // OpenTIP's limit
        private const int PollIntervalMs = 5000;
        private const int MaxPollAttempts = 60; // up to ~5 minutes for a result

        public string Name => "OpenTIP";

        public long MaxFileSizeBytes => FileSizeLimit;

        private static string ApiKey => SettingsStore.Load().OpenTipApiKey;

        private static HttpClient CreateClient()
        {
            var apiKey = ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new MissingApiKeyException("OpenTIP");
            }

            var client = HttpClientFactory.Create(BaseUrl, TimeSpan.FromMinutes(5));
            client.DefaultRequestHeaders.Add("x-api-key", apiKey);
            return client;
        }

        public async Task<ScanResult> LookupByHashAsync(string sha256, CancellationToken ct)
        {
            using (var client = CreateClient())
            {
                var response = await client.GetAsync("search/hash?request=" + sha256, ct).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null; // no data on OpenTIP yet, the caller will upload the file
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
                return Error("File exceeds 256MB - OpenTIP does not support uploads this large.");
            }

            using (var client = CreateClient())
            {
                progress?.Report(0);

                string fileHash;
                using (var fileStream = File.OpenRead(filePath))
                using (var content = new ProgressStreamContent(fileStream, uploaded =>
                {
                    var pct = (int)(uploaded * 30L / fileInfo.Length); // upload takes up 0-30% of progress
                    progress?.Report(Math.Min(pct, 30));
                }))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    var encodedName = HttpUtility.UrlEncode(Path.GetFileName(filePath));

                    var submitResponse = await client.PostAsync("scan/file?filename=" + encodedName, content, ct).ConfigureAwait(false);
                    if (!submitResponse.IsSuccessStatusCode)
                    {
                        return Error(await DescribeErrorAsync(submitResponse).ConfigureAwait(false));
                    }

                    var submitJson = await submitResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var submitRoot = JObject.Parse(submitJson);
                    fileHash = submitRoot["FileGeneralInfo"]?["Sha256"]?.ToString();

                    // OpenTIP may return the full result right on first submit (the file was already scanned before)
                    if (string.IsNullOrEmpty(submitRoot["Zone"]?.ToString()) == false &&
                        submitRoot["FileGeneralInfo"]?["FileStatus"] != null)
                    {
                        progress?.Report(100);
                        return BuildScanResult(submitRoot);
                    }
                }

                progress?.Report(30);

                if (string.IsNullOrEmpty(fileHash))
                {
                    return Error("OpenTIP did not return a file hash after submission.");
                }

                for (var attempt = 0; attempt < MaxPollAttempts; attempt++)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(PollIntervalMs, ct).ConfigureAwait(false);

                    var pollResponse = await client.PostAsync("getresult/file?request=" + fileHash, null, ct).ConfigureAwait(false);
                    if (!pollResponse.IsSuccessStatusCode)
                    {
                        return Error(await DescribeErrorAsync(pollResponse).ConfigureAwait(false));
                    }

                    var pollJson = await pollResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var root = JObject.Parse(pollJson);
                    var status = root["Status"]?.ToString();

                    progress?.Report(30 + Math.Min(65, (attempt + 1) * 65 / MaxPollAttempts));

                    if (status == "complete")
                    {
                        progress?.Report(100);
                        return BuildScanResult(root);
                    }
                }

                return Error("Timed out waiting for a result from OpenTIP, please try again later.");
            }
        }

        public Task<QuotaInfo> GetQuotaAsync(CancellationToken ct)
        {
            // No public quota endpoint was found for OpenTIP in the official documentation.
            return Task.FromResult<QuotaInfo>(null);
        }

        public async Task<ScanResult> ScanUrlAsync(string url, IProgress<int> progress, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return Error("Please enter a URL.");
            }

            using (var client = CreateClient())
            {
                progress?.Report(0);

                var encodedUrl = HttpUtility.UrlEncode(url);
                var response = await client.GetAsync("search/url?request=" + encodedUrl, ct).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    progress?.Report(100);
                    return new ScanResult
                    {
                        SourceName = Name,
                        IsClean = true,
                        DetectionRatio = "No data",
                        EngineResults = new Dictionary<string, string>(),
                        TargetUrl = url,
                        ErrorMessage = null
                    };
                }

                if (!response.IsSuccessStatusCode)
                {
                    return Error(await DescribeErrorAsync(response).ConfigureAwait(false));
                }

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                progress?.Report(100);
                return BuildUrlScanResult(JObject.Parse(json), url);
            }
        }

        private ScanResult BuildUrlScanResult(JObject root, string url)
        {
            // OpenTIP's URL Lookup docs don't have a DetectionsInfo array like the file endpoint does -
            // just an overall Zone, so there's no per-engine list; reuse the same Zone -> IsClean
            // logic as the file flow.
            var zone = root["Zone"]?.ToString();
            bool isClean = zone != "Red" && zone != "Orange" && zone != "Yellow";

            return new ScanResult
            {
                SourceName = Name,
                IsClean = isClean,
                DetectionRatio = "Zone: " + (zone ?? "Unknown"),
                EngineResults = new Dictionary<string, string>(),
                TargetUrl = url,
                ErrorMessage = null
            };
        }

        private ScanResult BuildScanResult(JObject root)
        {
            var zone = root["Zone"]?.ToString();
            var fileGeneralInfo = root["FileGeneralInfo"];
            var detections = root["DetectionsInfo"] as JArray;

            var engineResults = new Dictionary<string, string>();
            if (detections != null)
            {
                var index = 0;
                foreach (var detection in detections)
                {
                    index++;
                    var method = detection["DetectionMethod"]?.ToString();
                    var detectionName = detection["DetectionName"]?.ToString();
                    var key = string.IsNullOrEmpty(method) ? $"Kaspersky #{index}" : method;
                    engineResults[key] = string.IsNullOrEmpty(detectionName) ? zone : detectionName;
                }
            }

            // Zone: Red = malware, Yellow = adware/other, Green = clean, Grey = not enough data
            bool isClean = zone != "Red" && zone != "Yellow";

            return new ScanResult
            {
                SourceName = Name,
                IsClean = isClean,
                DetectionRatio = engineResults.Count + " detections",
                EngineResults = engineResults,
                Sha256 = fileGeneralInfo?["Sha256"]?.ToString(),
                Md5 = fileGeneralInfo?["Md5"]?.ToString(),
                ErrorMessage = null
            };
        }

        private ScanResult Error(string message)
        {
            return new ScanResult { SourceName = Name, ErrorMessage = message };
        }

        private async Task<string> DescribeErrorAsync(HttpResponseMessage response)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                return "OpenTIP API key is invalid.";
            }

            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return "OpenTIP quota/rate limit exceeded, please try again later.";
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
                    detail = JObject.Parse(body)["message"]?.ToString() ?? JObject.Parse(body)["error"]?.ToString();
                }
                catch
                {
                    // body isn't valid JSON, ignore
                }
            }

            return detail ?? $"OpenTIP HTTP error {(int)response.StatusCode}: {response.ReasonPhrase}";
        }
    }
}
