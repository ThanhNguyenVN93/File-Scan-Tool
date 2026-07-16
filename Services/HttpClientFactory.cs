using System;
using System.Net;
using System.Net.Http;

namespace frm_scan_online.Services
{
    // Creates the HttpClient shared by all 3 providers, applying the proxy config from Settings
    // (System/Manual/None). "System" needs no extra work - the default HttpClientHandler
    // (UseProxy=true, no Proxy assigned) already uses Windows' system proxy (WinINet) as-is.
    public static class HttpClientFactory
    {
        public static HttpClient Create(string baseUrl, TimeSpan timeout)
        {
            var settings = SettingsStore.Load();
            var handler = new HttpClientHandler();

            switch (settings.ProxyMode)
            {
                case "None":
                    handler.UseProxy = false;
                    break;

                case "Manual":
                    var address = (settings.ProxyAddress ?? string.Empty).Trim();
                    if (!string.IsNullOrEmpty(address))
                    {
                        var proxy = new WebProxy(NormalizeAddress(address));
                        if (!string.IsNullOrEmpty(settings.ProxyUsername))
                        {
                            proxy.Credentials = new NetworkCredential(settings.ProxyUsername, settings.ProxyPassword);
                        }

                        handler.Proxy = proxy;
                        handler.UseProxy = true;
                    }
                    break;
            }

            return new HttpClient(handler) { BaseAddress = new Uri(baseUrl), Timeout = timeout };
        }

        // Lets the user enter either "host:port" or a full "http://host:port" - WebProxy needs a valid URI.
        private static Uri NormalizeAddress(string address)
        {
            if (address.IndexOf("://", StringComparison.Ordinal) < 0)
            {
                address = "http://" + address;
            }

            return new Uri(address);
        }
    }
}
