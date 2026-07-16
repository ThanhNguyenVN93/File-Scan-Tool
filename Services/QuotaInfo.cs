namespace frm_scan_online.Services
{
    // Very simple quota info shown as a bonus in SettingsForm after a successful key test.
    // Optional - a provider with no quota endpoint (or a failed call) returns null, which has no
    // effect on the main "Key is valid" result.
    public class QuotaInfo
    {
        public int Used { get; set; }
        public int Allowed { get; set; }
        public string Label { get; set; } // e.g. "requests today"
    }
}
