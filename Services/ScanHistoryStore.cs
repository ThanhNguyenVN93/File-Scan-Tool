using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using frm_scan_online.Models;

namespace frm_scan_online.Services
{
    // Stores successful scan history (failed scans are not saved) - plain JSON, not encrypted since
    // it holds no sensitive data (just file name/hash/result, never the file content itself).
    public static class ScanHistoryStore
    {
        private const int MaxEntries = 200;

        private static readonly string FilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FileScanTool", "history.json");

        public static List<ScanHistoryEntry> Load()
        {
            if (!File.Exists(FilePath))
            {
                return new List<ScanHistoryEntry>();
            }

            try
            {
                var json = File.ReadAllText(FilePath);
                return JsonConvert.DeserializeObject<List<ScanHistoryEntry>>(json) ?? new List<ScanHistoryEntry>();
            }
            catch
            {
                return new List<ScanHistoryEntry>();
            }
        }

        public static void Add(ScanHistoryEntry entry)
        {
            var list = Load();
            list.Insert(0, entry);

            if (list.Count > MaxEntries)
            {
                list.RemoveRange(MaxEntries, list.Count - MaxEntries);
            }

            Save(list);
        }

        public static void Clear()
        {
            Save(new List<ScanHistoryEntry>());
        }

        // Finds the most recent scan result for a given hash + source, still within maxAge - used to
        // answer from history instead of calling the API again (saves quota, faster).
        public static ScanHistoryEntry FindRecent(string sha256, string sourceName, TimeSpan maxAge)
        {
            var cutoff = DateTime.Now - maxAge;

            return Load()
                .Where(e => string.Equals(e.Sha256, sha256, StringComparison.OrdinalIgnoreCase)
                            && string.Equals(e.SourceName, sourceName, StringComparison.OrdinalIgnoreCase)
                            && e.Timestamp >= cutoff)
                .OrderByDescending(e => e.Timestamp)
                .FirstOrDefault();
        }

        private static void Save(List<ScanHistoryEntry> list)
        {
            var dir = Path.GetDirectoryName(FilePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            File.WriteAllText(FilePath, JsonConvert.SerializeObject(list));
        }
    }
}
