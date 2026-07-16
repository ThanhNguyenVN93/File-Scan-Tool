using System;
using System.Collections.Generic;
using System.IO;
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
    // VirusTotal API v3 - https://docs.virustotal.com/reference/overview
    public class VirusTotalProvider : IScanProvider
    {
        private const string BaseUrl = "https://www.virustotal.com/api/v3/";
        private const long FileSizeLimit = 32L * 1024 * 1024; // file size limit of the free API tier
        private const int PollIntervalMs = 20000; // free tier ~4 req/min, pace to ~3 req/min
        private const int MaxPollAttempts = 20; // up to ~6-7 minutes for a result

        public string Name => "VirusTotal";

        public long MaxFileSizeBytes => FileSizeLimit;

        private static string ApiKey => SettingsStore.Load().VirusTotalApiKey;

        private static HttpClient CreateClient()
        {
            var apiKey = ApiKey;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new MissingApiKeyException("VirusTotal");
            }

            var client = HttpClientFactory.Create(BaseUrl, TimeSpan.FromMinutes(5));
            client.DefaultRequestHeaders.Add("x-apikey", apiKey);
            return client;
        }

        public async Task<ScanResult> LookupByHashAsync(string sha256, CancellationToken ct)
        {
            using (var client = CreateClient())
            {
                var response = await client.GetAsync("files/" + sha256, ct).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null; // no existing result on VirusTotal yet, the caller will upload the file
                }

                if (!response.IsSuccessStatusCode)
                {
                    return Error(await DescribeErrorAsync(response).ConfigureAwait(false));
                }

                var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var data = JObject.Parse(json)["data"];
                var attributes = data["attributes"];
                return BuildScanResult(
                    attributes["last_analysis_stats"],
                    attributes["last_analysis_results"] as JObject,
                    attributes["sha256"]?.ToString() ?? sha256,
                    attributes["md5"]?.ToString());
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
                return Error("File exceeds 32MB - VirusTotal (free tier) does not support uploads this large.");
            }

            using (var client = CreateClient())
            {
                string analysisId;
                progress?.Report(0);

                using (var fileStream = File.OpenRead(filePath))
                using (var content = new MultipartFormDataContent())
                using (var progressContent = new ProgressStreamContent(fileStream, uploaded =>
                {
                    var pct = (int)(uploaded * 50L / fileInfo.Length); // upload takes up 0-50% of progress
                    progress?.Report(Math.Min(pct, 50));
                }))
                {
                    progressContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    content.Add(progressContent, "file", Path.GetFileName(filePath));

                    var uploadResponse = await client.PostAsync("files", content, ct).ConfigureAwait(false);
                    if (!uploadResponse.IsSuccessStatusCode)
                    {
                        return Error(await DescribeErrorAsync(uploadResponse).ConfigureAwait(false));
                    }

                    var uploadJson = await uploadResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    analysisId = JObject.Parse(uploadJson)["data"]?["id"]?.ToString();
                }

                progress?.Report(50);

                if (string.IsNullOrEmpty(analysisId))
                {
                    return Error("VirusTotal did not return an analysis id after uploading.");
                }

                for (var attempt = 0; attempt < MaxPollAttempts; attempt++)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(PollIntervalMs, ct).ConfigureAwait(false);

                    var analysisResponse = await client.GetAsync("analyses/" + analysisId, ct).ConfigureAwait(false);
                    if (!analysisResponse.IsSuccessStatusCode)
                    {
                        return Error(await DescribeErrorAsync(analysisResponse).ConfigureAwait(false));
                    }

                    var analysisJson = await analysisResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var root = JObject.Parse(analysisJson);
                    var attributes = root["data"]?["attributes"];
                    var status = attributes?["status"]?.ToString();

                    progress?.Report(50 + Math.Min(45, (attempt + 1) * 45 / MaxPollAttempts));

                    if (status == "completed")
                    {
                        progress?.Report(100);
                        var fileInfoMeta = root["meta"]?["file_info"];
                        return BuildScanResult(
                            attributes["stats"],
                            attributes["results"] as JObject,
                            fileInfoMeta?["sha256"]?.ToString(),
                            fileInfoMeta?["md5"]?.ToString());
                    }
                }

                return Error("Timed out waiting for a result from VirusTotal, please try again later.");
            }
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

                // Look up first using url_id (base64url, no padding) - if the URL was already
                // analyzed by anyone before, there's a result immediately, no need to resubmit
                // (saves quota, same pattern as the hash flow).
                var urlId = ToUrlId(url);
                var lookupResponse = await client.GetAsync("urls/" + urlId, ct).ConfigureAwait(false);

                if (lookupResponse.IsSuccessStatusCode)
                {
                    var lookupJson = await lookupResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var attrs = JObject.Parse(lookupJson)["data"]?["attributes"];
                    progress?.Report(100);
                    return BuildUrlScanResult(attrs?["last_analysis_stats"], attrs?["last_analysis_results"] as JObject, url);
                }

                if (lookupResponse.StatusCode != HttpStatusCode.NotFound)
                {
                    return Error(await DescribeErrorAsync(lookupResponse).ConfigureAwait(false));
                }

                progress?.Report(10);

                string analysisId;
                using (var formContent = new FormUrlEncodedContent(new[] { new KeyValuePair<string, string>("url", url) }))
                {
                    var submitResponse = await client.PostAsync("urls", formContent, ct).ConfigureAwait(false);
                    if (!submitResponse.IsSuccessStatusCode)
                    {
                        return Error(await DescribeErrorAsync(submitResponse).ConfigureAwait(false));
                    }

                    var submitJson = await submitResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    analysisId = JObject.Parse(submitJson)["data"]?["id"]?.ToString();
                }

                progress?.Report(20);

                if (string.IsNullOrEmpty(analysisId))
                {
                    return Error("VirusTotal did not return an analysis id after submitting the URL.");
                }

                for (var attempt = 0; attempt < MaxPollAttempts; attempt++)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(PollIntervalMs, ct).ConfigureAwait(false);

                    var analysisResponse = await client.GetAsync("analyses/" + analysisId, ct).ConfigureAwait(false);
                    if (!analysisResponse.IsSuccessStatusCode)
                    {
                        return Error(await DescribeErrorAsync(analysisResponse).ConfigureAwait(false));
                    }

                    var analysisJson = await analysisResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var root = JObject.Parse(analysisJson);
                    var attributes = root["data"]?["attributes"];
                    var status = attributes?["status"]?.ToString();

                    progress?.Report(20 + Math.Min(75, (attempt + 1) * 75 / MaxPollAttempts));

                    if (status == "completed")
                    {
                        progress?.Report(100);
                        return BuildUrlScanResult(attributes["stats"], attributes["results"] as JObject, url);
                    }
                }

                return Error("Timed out waiting for a result from VirusTotal, please try again later.");
            }
        }

        public async Task<QuotaInfo> GetQuotaAsync(CancellationToken ct)
        {
            // The official docs say {id} is a username, but many integrations pass the API key
            // itself as {id} to get their own quota - not 100% confirmed with a real key, so catch
            // errors and return null instead of breaking the main "Key is valid" result in
            // SettingsForm.
            try
            {
                using (var client = CreateClient())
                {
                    var response = await client.GetAsync("users/" + ApiKey, ct).ConfigureAwait(false);
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    var json = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var daily = JObject.Parse(json)["data"]?["attributes"]?["quotas"]?["api_requests_daily"];
                    var used = (int?)daily?["used"];
                    var allowed = (int?)daily?["allowed"];

                    if (used == null || allowed == null)
                    {
                        return null;
                    }

                    return new QuotaInfo { Used = used.Value, Allowed = allowed.Value, Label = "requests used today" };
                }
            }
            catch
            {
                return null;
            }
        }

        // VirusTotal uses id = base64url(url) without padding to look up a URL object via GET /urls/{id}.
        private static string ToUrlId(string url)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(url);
            return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_').TrimEnd('=');
        }

        private ScanResult BuildUrlScanResult(JToken stats, JObject engineResultsJson, string url)
        {
            int malicious = (int?)stats?["malicious"] ?? 0;
            int suspicious = (int?)stats?["suspicious"] ?? 0;
            int total = 0;
            if (stats is JObject statsObj)
            {
                foreach (var prop in statsObj.Properties())
                {
                    total += (int?)prop.Value ?? 0;
                }
            }

            var engineResults = new Dictionary<string, string>();
            if (engineResultsJson != null)
            {
                foreach (var prop in engineResultsJson.Properties())
                {
                    var category = prop.Value["category"]?.ToString();
                    if (category == "malicious" || category == "suspicious")
                    {
                        var verdict = prop.Value["result"]?.ToString();
                        engineResults[prop.Name] = string.IsNullOrEmpty(verdict) ? category : verdict;
                    }
                }
            }

            return new ScanResult
            {
                SourceName = Name,
                IsClean = malicious == 0 && suspicious == 0,
                DetectionRatio = $"{malicious + suspicious}/{total}",
                EngineResults = engineResults,
                TargetUrl = url,
                ErrorMessage = null
            };
        }

        private ScanResult BuildScanResult(JToken stats, JObject engineResultsJson, string sha256, string md5)
        {
            int malicious = (int?)stats?["malicious"] ?? 0;
            int suspicious = (int?)stats?["suspicious"] ?? 0;
            int total = 0;
            if (stats is JObject statsObj)
            {
                foreach (var prop in statsObj.Properties())
                {
                    total += (int?)prop.Value ?? 0;
                }
            }

            var engineResults = new Dictionary<string, string>();
            if (engineResultsJson != null)
            {
                foreach (var prop in engineResultsJson.Properties())
                {
                    var category = prop.Value["category"]?.ToString();
                    if (category == "malicious" || category == "suspicious")
                    {
                        var verdict = prop.Value["result"]?.ToString();
                        engineResults[prop.Name] = string.IsNullOrEmpty(verdict) ? category : verdict;
                    }
                }
            }

            return new ScanResult
            {
                SourceName = Name,
                IsClean = malicious == 0 && suspicious == 0,
                DetectionRatio = $"{malicious + suspicious}/{total}",
                EngineResults = engineResults,
                Sha256 = sha256,
                Md5 = md5,
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
                return "VirusTotal API key is invalid or lacks access.";
            }

            if ((int)response.StatusCode == 429)
            {
                return "VirusTotal quota/rate limit exceeded, please try again later.";
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
                    detail = JObject.Parse(body)["error"]?["message"]?.ToString();
                }
                catch
                {
                    // body isn't valid JSON, ignore
                }
            }

            return detail ?? $"VirusTotal HTTP error {(int)response.StatusCode}: {response.ReasonPhrase}";
        }
    }
}
