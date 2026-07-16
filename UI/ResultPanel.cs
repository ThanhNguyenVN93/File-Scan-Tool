using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using frm_scan_online.Models;
using frm_scan_online.Services;

namespace frm_scan_online.UI
{
    public partial class ResultPanel : UserControl
    {
        private static readonly Regex RatioPattern = new Regex(@"^(\d+)\s*/\s*(\d+)$");

        private static readonly Color ActiveColor = Color.FromArgb(0xef, 0x53, 0x50);
        private static readonly Color InactiveColor = Color.FromArgb(0x8a, 0x89, 0x86);
        private static readonly Color CleanActiveColor = Color.FromArgb(0xe8, 0xe6, 0xe2);

        private string _currentFilePath;
        private string _currentSourceName;
        private string _currentSha256;
        private ScanResult _lastResult;
        private DateTime _scanTimestamp;
        private bool _isUrlTarget;
        private string _currentUrl;

        // Original position/size of lblActionStatus while Delete/Quarantine are still visible (file
        // scan mode) - used to restore it after hiding them in URL scan mode.
        private static readonly Point FileActionStatusLocation = new Point(270, 4);
        private static readonly Size FileActionStatusSize = new Size(272, 30);
        private static readonly Point UrlActionStatusLocation = new Point(0, 4);
        private static readonly Size UrlActionStatusSize = new Size(542, 30);

        public event EventHandler RescanRequested;

        public ResultPanel()
        {
            InitializeComponent();
            lvEngines.Resize += (s, e) => ResizeVerdictColumn();
            ResizeVerdictColumn();
        }

        private void ResizeVerdictColumn()
        {
            var remaining = lvEngines.ClientSize.Width - colEngine.Width;
            colVerdict.Width = remaining > 0 ? remaining : colVerdict.Width;
        }

        public void ShowLoading(string sourceName, string filePath)
        {
            _currentFilePath = filePath;
            _currentSourceName = sourceName;
            _isUrlTarget = false;
            _currentUrl = null;

            lblStatus.Visible = true;
            pgbScan.Visible = true;
            lblStatus.Text = $"Looking up hash on {sourceName}...";
            pgbScan.Value = 0;

            lblErrorMessage.Visible = false;
            pnlInfectedContent.Visible = false;
            pnlCleanContent.Visible = false;
            lblTabInfected.Visible = false;
            lblTabClean.Visible = false;
            btnViewSource.Visible = false;
            btnExport.Visible = false;
            btnRescan.Visible = false;

            btnDelete.Enabled = true;
            btnQuarantine.Enabled = true;
            lblActionStatus.Text = string.Empty;
        }

        public void ShowLoadingForUrl(string sourceName, string url)
        {
            _currentFilePath = null;
            _currentUrl = url;
            _isUrlTarget = true;
            _currentSourceName = sourceName;

            lblStatus.Visible = true;
            pgbScan.Visible = true;
            lblStatus.Text = $"Looking up this URL on {sourceName}...";
            pgbScan.Value = 0;

            lblErrorMessage.Visible = false;
            pnlInfectedContent.Visible = false;
            pnlCleanContent.Visible = false;
            lblTabInfected.Visible = false;
            lblTabClean.Visible = false;
            btnViewSource.Visible = false;
            btnExport.Visible = false;
            btnRescan.Visible = false;

            lblActionStatus.Text = string.Empty;
        }

        public void UpdateProgress(int percent, string sourceName)
        {
            pgbScan.Value = Math.Max(0, Math.Min(100, percent));
            lblStatus.Text = $"Scanning on {sourceName}... {percent}%";
        }

        public void ShowResult(ScanResult result, DateTime? cachedAt = null)
        {
            if (result == null || result.ErrorMessage != null)
            {
                ShowError(result?.SourceName, result?.ErrorMessage ?? "No result received.");
                return;
            }

            _lastResult = result;
            _scanTimestamp = cachedAt ?? DateTime.Now;

            pgbScan.Visible = false;
            lblErrorMessage.Visible = false;

            if (cachedAt.HasValue)
            {
                lblStatus.Visible = true;
                lblStatus.Text = $"From history - scanned {cachedAt.Value:g}";
                btnRescan.Visible = true;
            }
            else
            {
                lblStatus.Visible = false;
                btnRescan.Visible = false;
            }

            var detectedCount = result.EngineResults.Count;
            var match = RatioPattern.Match(result.DetectionRatio ?? string.Empty);
            int? total = match.Success ? int.Parse(match.Groups[2].Value) : (int?)null;
            var cleanCount = total.HasValue ? total.Value - detectedCount : (int?)null;

            lblTabInfected.Visible = true;
            lblTabClean.Visible = true;
            lblTabInfected.Text = $"Infected ({detectedCount})";
            lblTabClean.Text = cleanCount.HasValue ? $"Clean ({cleanCount})" : "Clean";

            lvEngines.Items.Clear();
            foreach (var kv in result.EngineResults)
            {
                var item = new ListViewItem(kv.Key);
                item.SubItems.Add(kv.Value);
                lvEngines.Items.Add(item);
            }

            lblCleanSummary.Text = total.HasValue
                ? $"{cleanCount}/{total} engines ({(cleanCount.Value * 100 / total.Value)}%) found nothing suspicious."
                : "This file is rated clean - no detailed ratio data available from this source.";

            if (_isUrlTarget)
            {
                lblHashSha256.Text = "URL: " + (result.TargetUrl ?? _currentUrl);
                lblHashMd5.Text = string.Empty;
                _currentSha256 = null;
                btnViewSource.Visible = false;
            }
            else
            {
                lblHashSha256.Text = "SHA-256: " + result.Sha256;
                lblHashMd5.Text = "MD5: " + result.Md5;
                _currentSha256 = result.Sha256;
                btnViewSource.Visible = !string.IsNullOrEmpty(result.Sha256);
            }

            // There's no local file in URL scan mode, so hide Delete/Quarantine (a URL can't be
            // deleted/quarantined) and let lblActionStatus stretch to fill the remaining width.
            btnDelete.Visible = !_isUrlTarget;
            btnQuarantine.Visible = !_isUrlTarget;
            btnDelete.Enabled = true;
            btnQuarantine.Enabled = true;
            lblActionStatus.Location = _isUrlTarget ? UrlActionStatusLocation : FileActionStatusLocation;
            lblActionStatus.Size = _isUrlTarget ? UrlActionStatusSize : FileActionStatusSize;

            btnExport.Visible = true;

            SelectTab(detectedCount > 0);
        }

        public void ShowError(string sourceName, string message)
        {
            lblStatus.Visible = false;
            pgbScan.Visible = false;
            pnlInfectedContent.Visible = false;
            pnlCleanContent.Visible = false;
            lblTabInfected.Visible = false;
            lblTabClean.Visible = false;

            lblErrorMessage.Visible = true;
            lblErrorMessage.Text = (string.IsNullOrEmpty(sourceName) ? string.Empty : sourceName + ": ") + message;

            lblHashSha256.Text = string.Empty;
            lblHashMd5.Text = string.Empty;
            btnViewSource.Visible = false;
            btnExport.Visible = false;
            btnRescan.Visible = false;
        }

        private void btnRescan_Click(object sender, EventArgs e)
        {
            RescanRequested?.Invoke(this, EventArgs.Empty);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (_lastResult == null)
            {
                return;
            }

            using (var dialog = new SaveFileDialog
            {
                Title = "Export scan report",
                Filter = "Text file (*.txt)|*.txt",
                FileName = Path.GetFileNameWithoutExtension(_currentFilePath ?? "scan-report") + "-report.txt"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    File.WriteAllText(dialog.FileName, BuildReportText());
                    lblActionStatus.Text = "Report exported.";
                    lblActionStatus.ForeColor = CleanActiveColor;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not export report: " + ex.Message,
                        "File Scan Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string BuildReportText()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("File Scan Tool - Scan Report");
            sb.AppendLine("=============================");
            sb.AppendLine(_isUrlTarget
                ? "URL: " + (_lastResult.TargetUrl ?? _currentUrl)
                : "File: " + Path.GetFileName(_currentFilePath ?? "(unknown)"));
            sb.AppendLine("Source: " + _lastResult.SourceName);
            sb.AppendLine("Scanned: " + _scanTimestamp.ToString("g"));
            sb.AppendLine("Result: " + (_lastResult.IsClean ? "Clean" : "Infected") + " (" + _lastResult.DetectionRatio + ")");
            if (!_isUrlTarget)
            {
                sb.AppendLine("SHA-256: " + _lastResult.Sha256);
                sb.AppendLine("MD5: " + _lastResult.Md5);
            }

            if (_lastResult.EngineResults.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Detected by:");
                foreach (var kv in _lastResult.EngineResults)
                {
                    sb.AppendLine("- " + kv.Key + ": " + kv.Value);
                }
            }

            return sb.ToString();
        }

        private void lblTabInfected_Click(object sender, EventArgs e)
        {
            SelectTab(true);
        }

        private void lblTabClean_Click(object sender, EventArgs e)
        {
            SelectTab(false);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
            {
                lblActionStatus.Text = "File not found to delete.";
                return;
            }

            var confirm = MessageBox.Show(this,
                $"Permanently delete \"{Path.GetFileName(_currentFilePath)}\"? This action cannot be undone.",
                "Confirm file deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                File.Delete(_currentFilePath);
                lblActionStatus.Text = "File deleted.";
                lblActionStatus.ForeColor = ActiveColor;
                btnDelete.Enabled = false;
                btnQuarantine.Enabled = false;
            }
            catch (Exception ex)
            {
                lblActionStatus.Text = "Could not delete: " + ex.Message;
                lblActionStatus.ForeColor = ActiveColor;
            }
        }

        private void btnQuarantine_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath) || !File.Exists(_currentFilePath))
            {
                lblActionStatus.Text = "File not found to quarantine.";
                return;
            }

            var confirm = MessageBox.Show(this,
                $"Move \"{Path.GetFileName(_currentFilePath)}\" to the quarantine folder? The file will no longer be at its original location.",
                "Confirm file quarantine", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
            {
                return;
            }

            try
            {
                var destPath = QuarantineStore.Move(_currentFilePath);
                lblActionStatus.Text = "Quarantined: " + Path.GetFileName(destPath);
                lblActionStatus.ForeColor = CleanActiveColor;
                btnDelete.Enabled = false;
                btnQuarantine.Enabled = false;
            }
            catch (Exception ex)
            {
                lblActionStatus.Text = "Could not quarantine: " + ex.Message;
                lblActionStatus.ForeColor = ActiveColor;
            }
        }

        private void btnViewSource_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentSha256))
            {
                return;
            }

            string url;
            switch (_currentSourceName)
            {
                case "VirusTotal":
                    url = $"https://www.virustotal.com/gui/file/{_currentSha256}";
                    break;
                case "MetaDefender":
                    url = $"https://metadefender.com/results/hash/{_currentSha256}";
                    break;
                case "OpenTIP":
                    url = $"https://opentip.kaspersky.com/{_currentSha256}/results?tab=lookup";
                    break;
                default:
                    return;
            }

            BrowserLauncher.Open(url);
        }

        private void SelectTab(bool infected)
        {
            pnlInfectedContent.Visible = infected;
            pnlCleanContent.Visible = !infected;

            lblTabInfected.ForeColor = infected ? ActiveColor : InactiveColor;
            lblTabInfected.Font = new Font(lblTabInfected.Font, infected ? FontStyle.Bold : FontStyle.Regular);

            lblTabClean.ForeColor = infected ? InactiveColor : CleanActiveColor;
            lblTabClean.Font = new Font(lblTabClean.Font, infected ? FontStyle.Regular : FontStyle.Bold);
        }
    }
}
