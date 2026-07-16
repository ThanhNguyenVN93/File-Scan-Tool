using System;
using System.IO;

namespace frm_scan_online.Services
{
    // Moves a suspicious file out of its original location into a dedicated folder instead of
    // deleting it outright (lets the user restore it if the result turns out to be a false positive).
    public static class QuarantineStore
    {
        private static readonly string QuarantineDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileScanTool", "Quarantine");

        public static string Move(string filePath)
        {
            if (!Directory.Exists(QuarantineDir))
            {
                Directory.CreateDirectory(QuarantineDir);
            }

            var fileName = Path.GetFileName(filePath);
            var destPath = Path.Combine(QuarantineDir, DateTime.Now.ToString("yyyyMMdd_HHmmss_") + fileName);

            File.Move(filePath, destPath);
            return destPath;
        }
    }
}
