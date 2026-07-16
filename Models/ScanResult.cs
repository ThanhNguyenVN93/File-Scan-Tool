using System.Collections.Generic;

namespace frm_scan_online.Models
{
    public class ScanResult
    {
        public string SourceName { get; set; }
        public bool IsClean { get; set; }
        public string DetectionRatio { get; set; } // e.g. "2/68"
        public Dictionary<string, string> EngineResults { get; set; } = new Dictionary<string, string>();
        public string Sha256 { get; set; }
        public string Md5 { get; set; }
        public string TargetUrl { get; set; } // only set for a URL scan result, null for a file scan
        public string ErrorMessage { get; set; } // null on success
    }
}
