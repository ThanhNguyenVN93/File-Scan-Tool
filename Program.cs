using System;
using System.Linq;
using System.Windows.Forms;
using frm_scan_online.Bootstrapper;
using frm_scan_online.Services;
using frm_scan_online.UI;

namespace frm_scan_online
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!EnsureDotNetInstalled())
            {
                return;
            }

            var startHidden = args.Contains(StartupRegistration.SilentArg, StringComparer.OrdinalIgnoreCase);
            Application.Run(new MainBar(startHidden));
        }

        private static bool EnsureDotNetInstalled()
        {
            BootstrapResult result;
            try
            {
                result = DotNetBootstrapper.Ensure();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Could not check/install the required .NET Framework:\n" + ex.Message,
                    "File Scan Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            switch (result)
            {
                case BootstrapResult.AlreadySatisfied:
                case BootstrapResult.InstalledSuccessfully:
                    return true;

                case BootstrapResult.NeedsRestart:
                    MessageBox.Show(
                        ".NET Framework was installed successfully. Please restart your computer to use File Scan Tool.",
                        "File Scan Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return false;

                case BootstrapResult.UserCancelledElevation:
                    MessageBox.Show(
                        "File Scan Tool needs Admin rights to install the missing .NET Framework. Please reopen the app and allow the UAC prompt.",
                        "File Scan Tool", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;

                default:
                    MessageBox.Show(
                        "Could not install the required .NET Framework after several attempts. Please contact support.",
                        "File Scan Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
            }
        }
    }
}
