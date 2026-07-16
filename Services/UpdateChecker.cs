using System;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace frm_scan_online.Services
{
    public class UpdateInfo
    {
        public string Version { get; set; }
        public string Url { get; set; }
    }

    // Checks for a newer version via the GitHub Releases API. Fill in GitHubRepo (as "owner/repo")
    // once a real repo exists to enable this feature - leaving it blank makes CheckAsync return null
    // immediately, with no network call and no error.
    public static class UpdateChecker
    {
        private const string GitHubRepo = ""; // e.g. "ThanhCode93/frm_scan_online"

        public static async Task<UpdateInfo> CheckAsync(CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(GitHubRepo))
            {
                return null;
            }

            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                {
                    client.DefaultRequestHeaders.UserAgent.ParseAdd("FileScanTool");

                    var json = await client.GetStringAsync($"https://api.github.com/repos/{GitHubRepo}/releases/latest");
                    var obj = JObject.Parse(json);
                    var tagName = obj["tag_name"]?.ToString();
                    var htmlUrl = obj["html_url"]?.ToString();

                    var remoteVersion = ParseVersion(tagName);
                    var currentVersion = GetCurrentVersion();

                    if (remoteVersion != null && remoteVersion > currentVersion)
                    {
                        return new UpdateInfo { Version = tagName, Url = htmlUrl };
                    }
                }
            }
            catch
            {
                // network/parse error - ignore, don't bother the user with an unexpected failure
            }

            return null;
        }

        public static Version GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        private static Version ParseVersion(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return null;
            }

            var cleaned = tagName.TrimStart('v', 'V');
            return Version.TryParse(cleaned, out var version) ? version : null;
        }
    }
}
