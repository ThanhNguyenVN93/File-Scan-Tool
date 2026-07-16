using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;

namespace frm_scan_online.Bootstrapper
{
    public enum BootstrapResult
    {
        AlreadySatisfied,
        InstalledSuccessfully,
        NeedsRestart,
        InstallFailed,
        UserCancelledElevation
    }

    // Checks for .NET Framework >= 4.7.2 (Release >= 461808), installs it if missing.
    // Only requests UAC when an install is actually needed (the whole app doesn't run as
    // requireAdministrator, to avoid a UAC prompt every launch on machines that already have .NET -
    // which is most users).
    public static class DotNetBootstrapper
    {
        public const int MinReleaseVersion = 461808; // .NET Framework 4.7.2

        private const string RegistryKeyPath = @"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full";
        private const int MaxInstallAttempts = 2;

        private static string BaseDir => AppDomain.CurrentDomain.BaseDirectory;
        private static string InstallerPath => Path.Combine(BaseDir, "Redist", "NetFx48Installer.exe");

        public static bool IsRequiredVersionInstalled()
        {
            using (var key = Registry.LocalMachine.OpenSubKey(RegistryKeyPath))
            {
                var release = key?.GetValue("Release");
                return release is int releaseValue && releaseValue >= MinReleaseVersion;
            }
        }

        public static BootstrapResult Ensure()
        {
            if (IsRequiredVersionInstalled())
            {
                return BootstrapResult.AlreadySatisfied;
            }

            if (!File.Exists(InstallerPath))
            {
                throw new FileNotFoundException(
                    "Could not find the offline .NET Framework installer. Place NetFx48Installer.exe " +
                    "in a Redist\\ folder next to the app's .exe before running it.", InstallerPath);
            }

            for (var attempt = 1; attempt <= MaxInstallAttempts; attempt++)
            {
                var exitCode = RunElevated(InstallerPath, "/quiet /norestart");

                if (exitCode == 0)
                {
                    return IsRequiredVersionInstalled() ? BootstrapResult.InstalledSuccessfully : BootstrapResult.InstallFailed;
                }

                if (exitCode == 3010)
                {
                    return BootstrapResult.NeedsRestart;
                }

                if (exitCode == UacDeniedExitCode)
                {
                    return BootstrapResult.UserCancelledElevation;
                }

                // Install failed for a reason related to shared system components (commonly seen on
                // stripped-down Win7 builds), not because .NET 3.5 is missing - try a repair then
                // reinstall before giving up.
                if (attempt < MaxInstallAttempts)
                {
                    RunSystemRepair();
                }
            }

            return BootstrapResult.InstallFailed;
        }

        private const int UacDeniedExitCode = -1;

        private static void RunSystemRepair()
        {
            // Same mechanism used by Dotnet-35-Install-And-Fix-Tool to repair install failures on
            // stripped-down Windows 7 builds: reset the Windows Update service then sfc/DISM. That repo
            // only ships a GUI with no silent CLI, so this runs the system commands directly instead
            // of shelling out to the external tool.
            RunElevated("cmd.exe",
                "/c net stop wuauserv & net stop bits & net start bits & net start wuauserv & " +
                "sfc /scannow & DISM /Online /Cleanup-Image /RestoreHealth");
        }

        private static int RunElevated(string fileName, string arguments)
        {
            var psi = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                using (var process = Process.Start(psi))
                {
                    process.WaitForExit();
                    return process.ExitCode;
                }
            }
            catch (Win32Exception)
            {
                // user clicked "No" on the UAC prompt
                return UacDeniedExitCode;
            }
        }
    }
}
