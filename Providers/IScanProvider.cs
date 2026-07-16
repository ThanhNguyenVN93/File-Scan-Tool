using System;
using System.Threading;
using System.Threading.Tasks;
using frm_scan_online.Models;
using frm_scan_online.Services;

namespace frm_scan_online.Providers
{
    public interface IScanProvider
    {
        string Name { get; }

        long MaxFileSizeBytes { get; }

        Task<ScanResult> LookupByHashAsync(string sha256, CancellationToken ct);

        Task<ScanResult> UploadAndScanAsync(string filePath, IProgress<int> progress, CancellationToken ct);

        // Scans a URL instead of a file. Sources that don't support it (e.g. MetaDefender - endpoint
        // schema not verified) return a ScanResult with a clear ErrorMessage instead of guessing the
        // response shape.
        Task<ScanResult> ScanUrlAsync(string url, IProgress<int> progress, CancellationToken ct);

        // Remaining quota info, shown as a bonus only - returns null if the source has no quota
        // endpoint or the call fails (must never let an error here break the main "Key is valid"
        // result in SettingsForm).
        Task<QuotaInfo> GetQuotaAsync(CancellationToken ct);
    }
}
