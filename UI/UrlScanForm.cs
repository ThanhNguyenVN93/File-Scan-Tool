using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using frm_scan_online.Providers;

namespace frm_scan_online.UI
{
    public partial class UrlScanForm : Form
    {
        private static readonly Color TextColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
        private static readonly Color MutedColor = Color.FromArgb(0x8a, 0x89, 0x86);
        private static readonly Color FieldBg = Color.FromArgb(0x3a, 0x39, 0x36);
        private static readonly Color AccentColor = Color.FromArgb(0xd8, 0x5a, 0x30);
        private static readonly Color ErrColor = Color.FromArgb(0xef, 0x53, 0x50);

        private readonly Guna.UI2.WinForms.Guna2TextBox _txtUrl;
        private readonly Guna.UI2.WinForms.Guna2ComboBox _cmbSource;
        private readonly Guna.UI2.WinForms.Guna2Button _btnScan;
        private readonly Label _lblError;

        private ResultPanelForm _resultPanelForm;
        private CancellationTokenSource _cts;

        public UrlScanForm()
        {
            InitializeComponent();

            var lblInstruction = new Label
            {
                Text = "Enter a URL to check (the URL itself is sent to the selected source):",
                Location = new Point(16, 16),
                Size = new Size(428, 20),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = MutedColor,
                BackColor = BackColor
            };

            _txtUrl = new Guna.UI2.WinForms.Guna2TextBox
            {
                Location = new Point(16, 44),
                Size = new Size(300, 34),
                BorderRadius = 8,
                FillColor = FieldBg,
                BorderColor = FieldBg,
                ForeColor = TextColor,
                PlaceholderText = "e.g. http://example.com/page",
                PlaceholderForeColor = MutedColor
            };

            _cmbSource = new Guna.UI2.WinForms.Guna2ComboBox
            {
                Location = new Point(324, 44),
                Size = new Size(120, 34),
                BorderRadius = 8,
                FillColor = FieldBg,
                BorderColor = FieldBg,
                ForeColor = TextColor,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbSource.Items.AddRange(new object[] { "VirusTotal", "OpenTIP" });
            _cmbSource.SelectedIndex = 0;

            _btnScan = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(16, 90),
                Size = new Size(100, 34),
                BorderRadius = 8,
                FillColor = AccentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = "Scan"
            };
            _btnScan.Click += async (s, e) => await ScanAsync();

            var lblNote = new Label
            {
                Text = "MetaDefender isn't offered here yet - its URL scan response format hasn't been verified.",
                Location = new Point(128, 94),
                Size = new Size(316, 32),
                Font = new Font("Segoe UI", 7.5F),
                ForeColor = MutedColor,
                BackColor = BackColor
            };

            _lblError = new Label
            {
                Location = new Point(16, 128),
                Size = new Size(428, 18),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = ErrColor,
                BackColor = BackColor,
                Visible = false
            };

            Controls.Add(lblInstruction);
            Controls.Add(_txtUrl);
            Controls.Add(_cmbSource);
            Controls.Add(_btnScan);
            Controls.Add(lblNote);
            Controls.Add(_lblError);

            FormClosed += (s, e) =>
            {
                _cts?.Cancel();
                _resultPanelForm?.Close();
            };
        }

        private IScanProvider GetSelectedProvider()
        {
            switch (_cmbSource.SelectedItem?.ToString())
            {
                case "VirusTotal":
                    return new VirusTotalProvider();
                case "OpenTIP":
                    return new OpenTipProvider();
                default:
                    return null;
            }
        }

        private void EnsureResultPanelForm()
        {
            if (_resultPanelForm != null && !_resultPanelForm.IsDisposed)
            {
                return;
            }

            _resultPanelForm = new ResultPanelForm();
        }

        private async Task ScanAsync()
        {
            var url = _txtUrl.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                _lblError.Text = "Please enter a URL.";
                _lblError.Visible = true;
                return;
            }

            if (url.IndexOf("://", StringComparison.Ordinal) < 0)
            {
                url = "http://" + url;
            }

            var provider = GetSelectedProvider();
            if (provider == null)
            {
                return;
            }

            _lblError.Visible = false;
            EnsureResultPanelForm();

            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            _btnScan.Enabled = false;
            _resultPanelForm.ShowBelow(this);
            _resultPanelForm.Panel.ShowLoadingForUrl(provider.Name, url);

            var progress = new Progress<int>(pct => _resultPanelForm.Panel.UpdateProgress(pct, provider.Name));

            try
            {
                var result = await provider.ScanUrlAsync(url, progress, ct);
                _resultPanelForm.Panel.ShowResult(result);
            }
            catch (MissingApiKeyException ex)
            {
                _resultPanelForm.Hide();
                _lblError.Text = $"{ex.ProviderName} API key is not set - open \"API key settings...\" from the pill's right-click menu first.";
                _lblError.Visible = true;
            }
            catch (OperationCanceledException)
            {
                // cancelled because the form was closed or Scan was clicked again mid-scan
            }
            catch (Exception ex)
            {
                _resultPanelForm.Panel.ShowError(provider.Name, "Unexpected error: " + ex.Message);
            }
            finally
            {
                _btnScan.Enabled = true;
            }
        }
    }
}
