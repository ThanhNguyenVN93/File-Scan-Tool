using System.Diagnostics;

namespace frm_scan_online.Services
{
    public static class BrowserLauncher
    {
        public static void Open(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                // ignore if the default browser could not be opened
            }
        }
    }
}
