using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace frm_scan_online.Services
{
    public class ApiKeySettings
    {
        public string VirusTotalApiKey { get; set; } = string.Empty;
        public string MetaDefenderApiKey { get; set; } = string.Empty;
        public string OpenTipApiKey { get; set; } = string.Empty;
        public bool HasShownTrayOnboarding { get; set; }

        // "System" (default, uses the system proxy the way HttpClient already does), "Manual" (proxy
        // entered by hand), "None" (proxy off, always go direct).
        public string ProxyMode { get; set; } = "System";
        public string ProxyAddress { get; set; } = string.Empty; // host:port format
        public string ProxyUsername { get; set; } = string.Empty;
        public string ProxyPassword { get; set; } = string.Empty;
    }

    // Stores API keys encrypted with Windows DPAPI (only the current machine/user can decrypt them),
    // no longer dependent on App.config (which needs a rebuild before any change takes effect - not
    // workable for letting the user type in a key while running the app).
    public static class SettingsStore
    {
        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileScanTool", "settings.dat");

        private static ApiKeySettings _cache;

        public static ApiKeySettings Load()
        {
            if (_cache != null)
            {
                return _cache;
            }

            if (File.Exists(FilePath))
            {
                try
                {
                    var encrypted = File.ReadAllBytes(FilePath);
                    var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
                    var json = Encoding.UTF8.GetString(decrypted);
                    _cache = JsonConvert.DeserializeObject<ApiKeySettings>(json) ?? new ApiKeySettings();
                }
                catch
                {
                    _cache = new ApiKeySettings();
                }
            }
            else
            {
                _cache = MigrateFromAppConfig();
            }

            return _cache;
        }

        public static void Save(ApiKeySettings settings)
        {
            _cache = settings;

            var json = JsonConvert.SerializeObject(settings);
            var plain = Encoding.UTF8.GetBytes(json);
            var encrypted = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);

            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllBytes(FilePath, encrypted);
        }

        // Users may have typed a key into App.config before the Settings UI existed - automatically
        // migrate it into the new encrypted store so they don't have to re-enter it; runs only once
        // (when settings.dat doesn't exist yet).
        private static ApiKeySettings MigrateFromAppConfig()
        {
            var settings = new ApiKeySettings
            {
                VirusTotalApiKey = ConfigurationManager.AppSettings["VirusTotalApiKey"] ?? string.Empty,
                MetaDefenderApiKey = ConfigurationManager.AppSettings["MetaDefenderApiKey"] ?? string.Empty,
                OpenTipApiKey = ConfigurationManager.AppSettings["OpenTipApiKey"] ?? string.Empty
            };

            var hasAnyKey = !string.IsNullOrEmpty(settings.VirusTotalApiKey)
                             || !string.IsNullOrEmpty(settings.MetaDefenderApiKey)
                             || !string.IsNullOrEmpty(settings.OpenTipApiKey);

            if (hasAnyKey)
            {
                Save(settings);
            }

            return settings;
        }
    }
}
