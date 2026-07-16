using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using frm_scan_online.Models;
using frm_scan_online.Providers;
using frm_scan_online.Services;

namespace frm_scan_online.UI
{
    public partial class MainBar : Form
    {
        [DllImport("user32.dll")]
        private static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HTCAPTION = 0x2;

        private const int HotkeyId = 0x1;
        private const uint ModControl = 0x2;
        private const uint ModAlt = 0x1;
        private const uint VkS = 0x53;
        private const int WM_HOTKEY = 0x0312;

        private static readonly Color HashNormalColor = Color.FromArgb(0x9a, 0x99, 0x96);
        private static readonly Color HashWarningColor = Color.FromArgb(0xd8, 0x5a, 0x30);

        private static readonly TimeSpan HistoryCacheMaxAge = TimeSpan.FromDays(30);

        private CancellationTokenSource _hashCts;
        private CancellationTokenSource _scanCts;
        private string _currentFilePath;
        private SignatureInfo _currentSignature;
        private bool? _isPasswordProtectedZip;
        private ResultPanelForm _resultPanelForm;
        private readonly bool _startHidden;

        public FileHashInfo CurrentHash { get; private set; }

        public MainBar() : this(false)
        {
        }

        public MainBar(bool startHidden)
        {
            _startHidden = startHidden;
            InitializeComponent();
        }

        // Lets the app start without popping the pill onto the screen (used when Windows launches
        // the app automatically at sign-in, see Program.cs and StartupRegistration.SilentArg).
        protected override void SetVisibleCore(bool value)
        {
            if (_startHidden && !IsHandleCreated)
            {
                if (!IsHandleCreated)
                {
                    CreateHandle();
                }

                value = false;
            }

            base.SetVisibleCore(value);
        }

        // Registers the global Ctrl+Alt+S hotkey to quick-scan whatever file is in the clipboard
        // (e.g. just Ctrl+C'd from Explorer), works even while the pill is hidden (tray icon only).
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterHotKey(Handle, HotkeyId, ModControl | ModAlt, VkS);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HotkeyId)
            {
                HandleClipboardScanHotkey();
            }

            base.WndProc(ref m);
        }

        private void HandleClipboardScanHotkey()
        {
            if (!Clipboard.ContainsFileDropList())
            {
                trayIcon.ShowBalloonTip(4000, "File Scan Tool",
                    "No file found in the clipboard to scan (Ctrl+C a file in Explorer first).",
                    ToolTipIcon.Info);
                return;
            }

            var files = Clipboard.GetFileDropList();
            if (files.Count == 0)
            {
                return;
            }

            ShowMainBar();
            SelectFile(files[0]);
        }

        private UpdateInfo _pendingUpdate;

        private async void MainBar_Load(object sender, EventArgs e)
        {
            var workArea = Screen.PrimaryScreen.WorkingArea;
            Location = new System.Drawing.Point(
                workArea.Left + (workArea.Width - Width) / 2,
                workArea.Top + (workArea.Height - Height) / 2);

            ShowTrayOnboardingIfNeeded();
            await CheckForUpdatesAsync(silent: true);
        }

        private async Task CheckForUpdatesAsync(bool silent)
        {
            var info = await UpdateChecker.CheckAsync(CancellationToken.None);

            if (info != null)
            {
                _pendingUpdate = info;
                trayIcon.ShowBalloonTip(
                    8000,
                    "A new update is available",
                    $"Version {info.Version} is available. Click here to view it.",
                    ToolTipIcon.Info);
            }
            else if (!silent)
            {
                MessageBox.Show(this,
                    "You're on the latest version (or the update check feature hasn't been configured yet).",
                    "File Scan Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void trayIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            if (_pendingUpdate != null)
            {
                BrowserLauncher.Open(_pendingUpdate.Url);
                _pendingUpdate = null;
            }
        }

        private void ShowTrayOnboardingIfNeeded()
        {
            var settings = SettingsStore.Load();
            if (settings.HasShownTrayOnboarding)
            {
                return;
            }

            trayIcon.ShowBalloonTip(
                5000,
                "File Scan Tool",
                "File Scan Tool is ready. Right-click this icon for more options (API key, lock position, support the author...).",
                ToolTipIcon.Info);

            settings.HasShownTrayOnboarding = true;
            SettingsStore.Save(settings);
        }

        // Drags the borderless form via WM_NCLBUTTONDOWN/HTCAPTION - more reliable than
        // Guna2DragControl, since any background area (pill, hash bar, icon) forwards straight to
        // Windows without child controls (textbox/dropdown/button) swallowing MouseDown before it
        // reaches the Form.
        private void DragArea_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && !mnuTrayLockPosition.Checked)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HTCAPTION, 0);
            }
        }

        private void txtFilePath_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Title = "Select a file to scan";
                dialog.Filter = "All files (*.*)|*.*";
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SelectFile(dialog.FileName);
                }
            }
        }

        private void DragArea_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void DragArea_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            {
                SelectFile(files[0]);
            }
        }

        private void SelectFile(string filePath)
        {
            _currentFilePath = filePath;
            txtFilePath.Text = filePath;
            ComputeHashAsync(filePath);
        }

        private void cmbSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshSizeWarning();
        }

        private IScanProvider GetSelectedProvider()
        {
            switch (cmbSource.SelectedItem?.ToString())
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

        // Warns about the selected source's size limit as soon as a file is picked, instead of
        // waiting until Scan is clicked and only failing after the hash lookup step.
        private void RefreshSizeWarning()
        {
            if (CurrentHash == null || string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            var provider = GetSelectedProvider();
            if (provider == null)
            {
                return;
            }

            var fileSize = new FileInfo(_currentFilePath).Length;
            if (fileSize > provider.MaxFileSizeBytes)
            {
                var limitMb = provider.MaxFileSizeBytes / (1024 * 1024);
                lblHashPreview.Text = $"File exceeds the {limitMb}MB limit for {provider.Name} - Scan will fail";
                lblHashPreview.ForeColor = HashWarningColor;
                return;
            }

            if (_isPasswordProtectedZip == true)
            {
                lblHashPreview.Text = $"Password-protected ZIP - contents may not be scannable  •  SHA-256: {Elide(CurrentHash.Sha256)}";
                lblHashPreview.ForeColor = HashWarningColor;
                return;
            }

            var signaturePrefix = _currentSignature != null && _currentSignature.IsSigned
                ? $"Signed ({_currentSignature.Publisher})  •  "
                : "No embedded signature  •  ";

            lblHashPreview.Text = signaturePrefix + $"SHA-256: {Elide(CurrentHash.Sha256)}";
            lblHashPreview.ForeColor = HashNormalColor;
        }

        private async void ComputeHashAsync(string filePath)
        {
            _hashCts?.Cancel();
            _hashCts = new CancellationTokenSource();
            var ct = _hashCts.Token;

            CurrentHash = null;
            _currentSignature = null;
            _isPasswordProtectedZip = null;
            btnScan.Enabled = false;
            pnlHashBar.Visible = true;
            lblHashPreview.ForeColor = HashNormalColor;
            lblHashPreview.Text = "Computing hash...";
            toolTipHash.SetToolTip(lblHashPreview, string.Empty);

            try
            {
                var hash = await FileHasher.ComputeAsync(filePath, ct);
                if (ct.IsCancellationRequested)
                {
                    return;
                }

                CurrentHash = hash;
                _currentSignature = SignatureChecker.Check(filePath);
                _isPasswordProtectedZip = string.Equals(Path.GetExtension(filePath), ".zip", StringComparison.OrdinalIgnoreCase)
                    ? ArchiveChecker.IsPasswordProtectedZip(filePath)
                    : null;

                var signatureLine = _currentSignature.IsSigned
                    ? "Signature: " + _currentSignature.Publisher
                    : "No embedded signature (some Windows system files use catalog signing, which can't be checked this way)";
                toolTipHash.SetToolTip(lblHashPreview,
                    $"SHA-256: {hash.Sha256}\nMD5: {hash.Md5}\nSHA-1: {hash.Sha1}\n{signatureLine}");

                RefreshSizeWarning();
                btnScan.Enabled = true;
            }
            catch (OperationCanceledException)
            {
                // cancelled because the user picked a different file while the hash was being computed
            }
            catch (Exception ex)
            {
                lblHashPreview.Text = "Could not compute hash: " + ex.Message;
            }
        }

        private static string Elide(string hex, int keep = 8)
        {
            return hex.Length <= keep * 2 ? hex : hex.Substring(0, keep) + "…" + hex.Substring(hex.Length - keep);
        }

        private void btnCopyHash_Click(object sender, EventArgs e)
        {
            if (CurrentHash == null)
            {
                return;
            }

            Clipboard.SetText($"SHA-256: {CurrentHash.Sha256}\nMD5: {CurrentHash.Md5}\nSHA-1: {CurrentHash.Sha1}");

            var originalText = btnCopyHash.Text;
            btnCopyHash.Text = "OK";
            var timer = new System.Windows.Forms.Timer { Interval = 1200 };
            timer.Tick += (s, args) =>
            {
                btnCopyHash.Text = originalText;
                timer.Stop();
                timer.Dispose();
            };
            timer.Start();
        }

        private void EnsureResultPanelForm()
        {
            if (_resultPanelForm != null && !_resultPanelForm.IsDisposed)
            {
                return;
            }

            _resultPanelForm = new ResultPanelForm();
            _resultPanelForm.Panel.RescanRequested += async (s, args) =>
            {
                // Re-reads whichever provider is selected at the moment Rescan is clicked (doesn't
                // reuse an old captured variable, since the user may have changed the source
                // dropdown after the cached result was shown).
                var currentProvider = GetSelectedProvider();
                if (currentProvider != null)
                {
                    await PerformScanAsync(currentProvider, forceRescan: true);
                }
            };
        }

        private async void btnScan_Click(object sender, EventArgs e)
        {
            if (CurrentHash == null || string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show(this, "Please select a file first.", "File Scan Tool",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var provider = GetSelectedProvider();
            if (provider == null)
            {
                MessageBox.Show(this, cmbSource.SelectedItem + " is not implemented yet.", "File Scan Tool",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            EnsureResultPanelForm();

            // Check local history first - if this exact hash + source was scanned within
            // HistoryCacheMaxAge, show it right away instead of calling the API (saves quota, faster).
            var cached = ScanHistoryStore.FindRecent(CurrentHash.Sha256, provider.Name, HistoryCacheMaxAge);
            if (cached != null)
            {
                var cachedResult = new ScanResult
                {
                    SourceName = cached.SourceName,
                    IsClean = cached.IsClean,
                    DetectionRatio = cached.DetectionRatio,
                    EngineResults = cached.EngineResults ?? new System.Collections.Generic.Dictionary<string, string>(),
                    Sha256 = cached.Sha256,
                    Md5 = cached.Md5,
                    ErrorMessage = null
                };

                _resultPanelForm.ShowBelow(this);
                _resultPanelForm.Panel.ShowResult(cachedResult, cached.Timestamp);
                return;
            }

            await PerformScanAsync(provider, forceRescan: false);
        }

        private async Task PerformScanAsync(IScanProvider provider, bool forceRescan)
        {
            if (CurrentHash == null || string.IsNullOrEmpty(_currentFilePath))
            {
                return;
            }

            EnsureResultPanelForm();

            _scanCts?.Cancel();
            _scanCts = new CancellationTokenSource();
            var ct = _scanCts.Token;

            btnScan.Enabled = false;

            _resultPanelForm.ShowBelow(this);
            _resultPanelForm.Panel.ShowLoading(provider.Name, _currentFilePath);

            var progress = new Progress<int>(pct => _resultPanelForm.Panel.UpdateProgress(pct, provider.Name));

            try
            {
                ScanResult result = null;

                if (!forceRescan)
                {
                    result = await provider.LookupByHashAsync(CurrentHash.Sha256, ct);
                }

                if (result == null)
                {
                    _resultPanelForm.Hide();
                    var fileName = Path.GetFileName(_currentFilePath);
                    var confirm = MessageBox.Show(this,
                        $"{provider.Name} has no existing result for this hash. The actual file \"{fileName}\" will be uploaded to " +
                        $"{provider.Name} for scanning (the file content will be stored by this third party).\n\nDo you want to continue?",
                        "Confirm file upload", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (confirm != DialogResult.Yes)
                    {
                        return;
                    }

                    _resultPanelForm.ShowBelow(this);
                    _resultPanelForm.Panel.ShowLoading(provider.Name, _currentFilePath);

                    result = await provider.UploadAndScanAsync(_currentFilePath, progress, ct);
                }

                _resultPanelForm.Panel.ShowResult(result);

                if (result.ErrorMessage == null)
                {
                    ScanHistoryStore.Add(new ScanHistoryEntry
                    {
                        Timestamp = DateTime.Now,
                        FileName = Path.GetFileName(_currentFilePath),
                        SourceName = provider.Name,
                        IsClean = result.IsClean,
                        DetectionRatio = result.DetectionRatio,
                        Sha256 = result.Sha256,
                        Md5 = result.Md5,
                        EngineResults = result.EngineResults
                    });

                    if (!result.IsClean)
                    {
                        trayIcon.ShowBalloonTip(
                            8000,
                            "Virus detected!",
                            $"{Path.GetFileName(_currentFilePath)}: {result.DetectionRatio} on {provider.Name}",
                            ToolTipIcon.Warning);
                    }
                }
            }
            catch (MissingApiKeyException ex)
            {
                _resultPanelForm.Hide();
                OpenSettings(ex.ProviderName);
            }
            catch (OperationCanceledException)
            {
                // user cancelled or picked a different file while a scan was in progress
            }
            catch (Exception ex)
            {
                _resultPanelForm.Panel.ShowError(provider.Name, "Unexpected error: " + ex.Message);
            }
            finally
            {
                btnScan.Enabled = true;
            }
        }

        private void mnuSettings_Click(object sender, EventArgs e)
        {
            OpenSettings(null);
        }

        private void mnuVerifyChecksum_Click(object sender, EventArgs e)
        {
            if (CurrentHash == null || string.IsNullOrEmpty(_currentFilePath))
            {
                MessageBox.Show(this, "Select a file first to verify its checksum.", "File Scan Tool",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (var form = new ChecksumForm(Path.GetFileName(_currentFilePath), CurrentHash))
            {
                form.ShowDialog(this);
            }
        }

        private void mnuScanUrl_Click(object sender, EventArgs e)
        {
            using (var form = new UrlScanForm())
            {
                form.ShowDialog(this);
            }
        }

        private void mnuBatchLookup_Click(object sender, EventArgs e)
        {
            using (var form = new BatchLookupForm())
            {
                form.ShowDialog(this);
            }
        }

        private void MainBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                Hide();
            }
        }

        private void MainBar_FormClosing(object sender, FormClosingEventArgs e)
        {
            trayIcon.Visible = false;
            UnregisterHotKey(Handle, HotkeyId);
        }

        private void trayIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left)
            {
                return;
            }

            if (Visible)
            {
                Hide();
            }
            else
            {
                ShowMainBar();
            }
        }

        private void trayIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowMainBar();
        }

        private void ShowMainBar()
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
            Activate();
        }

        private void trayMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            mnuTrayStartup.Checked = StartupRegistration.IsEnabled();
        }

        private void mnuTrayStartup_Click(object sender, EventArgs e)
        {
            StartupRegistration.SetEnabled(mnuTrayStartup.Checked);
        }

        private void mnuTrayFeedback_Click(object sender, EventArgs e)
        {
            BrowserLauncher.Open("https://forms.gle/ykzrNMSWWATDpGVk7");
        }

        private void mnuTrayDonate_Click(object sender, EventArgs e)
        {
            using (var form = new DonateForm())
            {
                form.ShowDialog(this);
            }
        }

        private void mnuTrayHistory_Click(object sender, EventArgs e)
        {
            using (var form = new HistoryForm())
            {
                form.ShowDialog(this);
            }
        }

        private async void mnuTrayCheckUpdate_Click(object sender, EventArgs e)
        {
            await CheckForUpdatesAsync(silent: false);
        }

        private void mnuTrayExit_Click(object sender, EventArgs e)
        {
            trayIcon.Visible = false;
            Application.Exit();
        }

        private void OpenSettings(string focusProvider)
        {
            using (var form = new SettingsForm())
            {
                if (!string.IsNullOrEmpty(focusProvider))
                {
                    form.FocusProvider(focusProvider);
                }

                form.ShowDialog(this);
            }
        }
    }
}
