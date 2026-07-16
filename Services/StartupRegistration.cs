using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace frm_scan_online.Services
{
    // Toggles launch-at-startup via the current user's registry Run key (no Admin rights needed).
    public static class StartupRegistration
    {
        // This flag is appended to the Run key value so Program.cs knows Windows launched the app
        // automatically (rather than the user opening it), and can stay minimized in the tray
        // instead of popping the pill up immediately.
        public const string SilentArg = "--silent";

        private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string ValueName = "FileScanTool";

        public static bool IsEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, false))
            {
                var value = key?.GetValue(ValueName) as string;
                return !string.IsNullOrEmpty(value)
                       && value.StartsWith("\"" + Application.ExecutablePath + "\"", StringComparison.OrdinalIgnoreCase);
            }
        }

        public static void SetEnabled(bool enabled)
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, true))
            {
                if (key == null)
                {
                    return;
                }

                if (enabled)
                {
                    key.SetValue(ValueName, "\"" + Application.ExecutablePath + "\" " + SilentArg);
                }
                else if (key.GetValue(ValueName) != null)
                {
                    key.DeleteValue(ValueName, false);
                }
            }
        }
    }
}
