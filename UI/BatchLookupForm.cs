using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using frm_scan_online.Models;
using frm_scan_online.Providers;
using frm_scan_online.Services;

namespace frm_scan_online.UI
{
    public partial class BatchLookupForm : Form
    {
        private static readonly System.Drawing.Color TextColor = System.Drawing.Color.FromArgb(0xe8, 0xe6, 0xe2);
        private static readonly System.Drawing.Color MutedColor = System.Drawing.Color.FromArgb(0x8a, 0x89, 0x86);
        private static readonly System.Drawing.Color FieldBg = System.Drawing.Color.FromArgb(0x3a, 0x39, 0x36);
        private static readonly System.Drawing.Color ListBg = System.Drawing.Color.FromArgb(0x2b, 0x2a, 0x28);
        private static readonly System.Drawing.Color AccentColor = System.Drawing.Color.FromArgb(0xd8, 0x5a, 0x30);
        private static readonly System.Drawing.Color CleanColor = System.Drawing.Color.FromArgb(0x4c, 0xaf, 0x50);
        private static readonly System.Drawing.Color InfectedColor = System.Drawing.Color.FromArgb(0xef, 0x53, 0x50);

        private readonly TextBox _txtHashes;
        private readonly Guna.UI2.WinForms.Guna2ComboBox _cmbSource;
        private readonly Guna.UI2.WinForms.Guna2Button _btnStart;
        private readonly Guna.UI2.WinForms.Guna2Button _btnCancel;
        private readonly Guna.UI2.WinForms.Guna2Button _btnExport;
        private readonly Label _lblProgress;
        private readonly ListView _lvResults;

        private CancellationTokenSource _cts;
        private readonly List<(string Hash, string Status, string Ratio)> _exportRows = new List<(string, string, string)>();

        public BatchLookupForm()
        {
            InitializeComponent();

            var lblInstruction = new Label
            {
                Text = "Paste one hash per line (MD5, SHA-1 or SHA-256) - looks up existing results only, no files are uploaded:",
                Location = new System.Drawing.Point(16, 12),
                Size = new System.Drawing.Size(480, 32),
                Font = new System.Drawing.Font("Segoe UI", 8.5F),
                ForeColor = MutedColor,
                BackColor = BackColor
            };

            _txtHashes = new TextBox
            {
                Location = new System.Drawing.Point(16, 48),
                Size = new System.Drawing.Size(480, 90),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = FieldBg,
                ForeColor = TextColor,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new System.Drawing.Font("Consolas", 8.5F)
            };

            _cmbSource = new Guna.UI2.WinForms.Guna2ComboBox
            {
                Location = new System.Drawing.Point(16, 146),
                Size = new System.Drawing.Size(150, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                BorderColor = FieldBg,
                ForeColor = TextColor,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbSource.Items.AddRange(new object[] { "VirusTotal", "MetaDefender", "OpenTIP" });
            _cmbSource.SelectedIndex = 0;

            _btnStart = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new System.Drawing.Point(176, 146),
                Size = new System.Drawing.Size(90, 32),
                BorderRadius = 8,
                FillColor = AccentColor,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold),
                Text = "Start"
            };
            _btnStart.Click += async (s, e) => await StartAsync();

            _btnCancel = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new System.Drawing.Point(274, 146),
                Size = new System.Drawing.Size(90, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                ForeColor = TextColor,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Text = "Cancel",
                Enabled = false
            };
            _btnCancel.Click += (s, e) => _cts?.Cancel();

            _btnExport = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new System.Drawing.Point(406, 146),
                Size = new System.Drawing.Size(90, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                ForeColor = TextColor,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Text = "Export",
                Enabled = false
            };
            _btnExport.Click += (s, e) => ExportResults();

            _lblProgress = new Label
            {
                Location = new System.Drawing.Point(16, 186),
                Size = new System.Drawing.Size(480, 18),
                Font = new System.Drawing.Font("Segoe UI", 8.5F),
                ForeColor = MutedColor,
                BackColor = BackColor
            };

            _lvResults = new ListView
            {
                Location = new System.Drawing.Point(16, 210),
                Size = new System.Drawing.Size(480, 210),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,
                HeaderStyle = ColumnHeaderStyle.Nonclickable,
                BackColor = ListBg,
                ForeColor = TextColor,
                BorderStyle = BorderStyle.None,
                Font = new System.Drawing.Font("Segoe UI", 8.5F)
            };
            _lvResults.Columns.Add("Hash", 220);
            _lvResults.Columns.Add("Status", 140);
            _lvResults.Columns.Add("Ratio", 110);

            var btnClose = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new System.Drawing.Point(406, 428),
                Size = new System.Drawing.Size(90, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                ForeColor = TextColor,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Text = "Close"
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(lblInstruction);
            Controls.Add(_txtHashes);
            Controls.Add(_cmbSource);
            Controls.Add(_btnStart);
            Controls.Add(_btnCancel);
            Controls.Add(_btnExport);
            Controls.Add(_lblProgress);
            Controls.Add(_lvResults);
            Controls.Add(btnClose);

            FormClosed += (s, e) => _cts?.Cancel();
        }

        private IScanProvider GetSelectedProvider()
        {
            switch (_cmbSource.SelectedItem?.ToString())
            {
                case "VirusTotal":
                    return new VirusTotalProvider();
                case "MetaDefender":
                    return new MetaDefenderProvider();
                case "OpenTIP":
                    return new OpenTipProvider();
                default:
                    return null;
            }
        }

        // Pacing between API calls per hash to avoid rate-limiting - VirusTotal's free tier is
        // limited to ~4 req/min, so it needs a much slower pace than the other two sources.
        private static int DelayMsFor(string providerName)
        {
            return providerName == "VirusTotal" ? 16000 : 3000;
        }

        private async Task StartAsync()
        {
            var lines = _txtHashes.Text
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => l.Length > 0)
                .Distinct()
                .ToList();

            if (lines.Count == 0)
            {
                _lblProgress.Text = "Paste at least one hash first.";
                return;
            }

            var provider = GetSelectedProvider();
            if (provider == null)
            {
                return;
            }

            _lvResults.Items.Clear();
            _exportRows.Clear();
            _btnStart.Enabled = false;
            _btnCancel.Enabled = true;
            _btnExport.Enabled = false;

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            try
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    ct.ThrowIfCancellationRequested();

                    var hash = lines[i];
                    _lblProgress.Text = $"Checking {i + 1}/{lines.Count} on {provider.Name}...";

                    if (!IsValidHashFormat(hash))
                    {
                        AddRow(hash, "Invalid format", string.Empty, MutedColor);
                    }
                    else
                    {
                        try
                        {
                            var result = await provider.LookupByHashAsync(hash, ct);
                            if (result == null)
                            {
                                AddRow(hash, "No cached result", string.Empty, MutedColor);
                            }
                            else if (result.ErrorMessage != null)
                            {
                                AddRow(hash, "Error: " + result.ErrorMessage, string.Empty, InfectedColor);
                            }
                            else
                            {
                                AddRow(hash, result.IsClean ? "Clean" : "Infected", result.DetectionRatio,
                                    result.IsClean ? CleanColor : InfectedColor);

                                ScanHistoryStore.Add(new ScanHistoryEntry
                                {
                                    Timestamp = DateTime.Now,
                                    FileName = hash,
                                    SourceName = provider.Name,
                                    IsClean = result.IsClean,
                                    DetectionRatio = result.DetectionRatio,
                                    Sha256 = result.Sha256,
                                    Md5 = result.Md5,
                                    EngineResults = result.EngineResults
                                });
                            }
                        }
                        catch (MissingApiKeyException ex)
                        {
                            _lblProgress.Text = $"{ex.ProviderName} API key is not set - open \"API key settings...\" first.";
                            break;
                        }
                    }

                    if (i < lines.Count - 1)
                    {
                        await Task.Delay(DelayMsFor(provider.Name), ct).ConfigureAwait(true);
                    }
                }

                _lblProgress.Text = $"Done - checked {lines.Count} hash(es) on {provider.Name}.";
            }
            catch (OperationCanceledException)
            {
                _lblProgress.Text = "Cancelled.";
            }
            finally
            {
                _btnStart.Enabled = true;
                _btnCancel.Enabled = false;
                _btnExport.Enabled = _exportRows.Count > 0;
            }
        }

        private static bool IsValidHashFormat(string hash)
        {
            if (hash.Length != 32 && hash.Length != 40 && hash.Length != 64)
            {
                return false;
            }

            foreach (var c in hash)
            {
                var isHex = (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
                if (!isHex)
                {
                    return false;
                }
            }

            return true;
        }

        private void AddRow(string hash, string status, string ratio, System.Drawing.Color color)
        {
            var item = new ListViewItem(hash) { ForeColor = color };
            item.SubItems.Add(status);
            item.SubItems.Add(ratio);
            _lvResults.Items.Add(item);
            item.EnsureVisible();

            _exportRows.Add((hash, status, ratio));
        }

        private void ExportResults()
        {
            using (var dialog = new SaveFileDialog
            {
                Title = "Export batch lookup results",
                Filter = "Text file (*.txt)|*.txt",
                FileName = "batch-hash-lookup.txt"
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    var sb = new System.Text.StringBuilder();
                    sb.AppendLine("File Scan Tool - Batch Hash Lookup");
                    sb.AppendLine("====================================");
                    sb.AppendLine("Source: " + _cmbSource.SelectedItem);
                    sb.AppendLine();
                    foreach (var row in _exportRows)
                    {
                        sb.AppendLine($"{row.Hash}  {row.Status}  {row.Ratio}".TrimEnd());
                    }

                    File.WriteAllText(dialog.FileName, sb.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, "Could not export results: " + ex.Message,
                        "File Scan Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
