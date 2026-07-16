using System;
using System.Collections.Generic;

namespace frm_scan_online.Models
{
    public class ScanHistoryEntry
    {
        public DateTime Timestamp { get; set; }
        public string FileName { get; set; }
        public string SourceName { get; set; }
        public bool IsClean { get; set; }
        public string DetectionRatio { get; set; }
        public string Sha256 { get; set; }
        public string Md5 { get; set; }
        public Dictionary<string, string> EngineResults { get; set; } = new Dictionary<string, string>();
    }
}
