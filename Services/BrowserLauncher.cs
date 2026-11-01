using System;
using System.Diagnostics;

namespace frm_scan_online.Services
{
    public static class BrowserLauncher
    {
        public static void Open(string url)
        {
            // One call site (the update checker) passes a URL that came from a network response
            // rather than a hardcoded string. ShellExecute treats this as "open" on an arbitrary
            // path, not strictly as a web navigation — a non-http(s) value (a local/UNC path, for
            // instance) could end up launching something instead of just opening a browser tab.
            // Restricting to http/https keeps this call doing only what every caller expects.
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true });
            }
            catch
            {
                // ignore if the default browser could not be opened
            }
        }
    }
}
