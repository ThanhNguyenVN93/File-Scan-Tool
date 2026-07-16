using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using frm_scan_online.Providers;
using frm_scan_online.Services;

namespace frm_scan_online.UI
{
    public partial class SettingsForm : Form
    {
        // SHA-256 of an empty string - used to check whether a key is still valid without needing a
        // real file. Any API response (even a 404 "not found") means the key authenticated
        // successfully; only 401/403 means the key itself is wrong.
        private const string ProbeHash = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855";

        private static readonly Color TextColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
        private static readonly Color MutedColor = Color.FromArgb(0x8a, 0x89, 0x86);
        private static readonly Color OkColor = Color.FromArgb(0x4c, 0xaf, 0x50);
        private static readonly Color ErrColor = Color.FromArgb(0xef, 0x53, 0x50);
        private static readonly Color FieldBg = Color.FromArgb(0x3a, 0x39, 0x36);
        private static readonly Color AmberColor = Color.FromArgb(0xf2, 0xa6, 0x23);

        private sealed class ProviderRow
        {
            public string ProviderKey;
            public Guna.UI2.WinForms.Guna2TextBox TxtKey;
            public Label LblStatus;
            public Func<IScanProvider> CreateProvider;
        }

        private readonly ProviderRow[] _rows;

        private Guna.UI2.WinForms.Guna2ComboBox _cmbProxyMode;
        private Guna.UI2.WinForms.Guna2TextBox _txtProxyAddress;
        private Guna.UI2.WinForms.Guna2TextBox _txtProxyUser;
        private Guna.UI2.WinForms.Guna2TextBox _txtProxyPass;

        private const int NetworkSectionTop = 16 + (3 * 116) + 8;

        public SettingsForm()
        {
            InitializeComponent();

            var settings = SettingsStore.Load();

            _rows = new[]
            {
                BuildRow(0, "VirusTotal", settings.VirusTotalApiKey,
                    "https://www.virustotal.com/gui/my-apikey", () => new VirusTotalProvider()),
                BuildRow(1, "MetaDefender", settings.MetaDefenderApiKey,
                    "https://my.opswat.com/", () => new MetaDefenderProvider()),
                BuildRow(2, "OpenTIP", settings.OpenTipApiKey,
                    "https://opentip.kaspersky.com/token", () => new OpenTipProvider())
            };

            BuildNetworkSection(settings);
            BuildFooter();
        }

        private void BuildNetworkSection(ApiKeySettings settings)
        {
            var top = NetworkSectionTop;

            var lblHeader = new Label
            {
                Text = "Network (proxy)",
                Location = new Point(16, top),
                Size = new Size(300, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextColor,
                BackColor = BackColor
            };

            _cmbProxyMode = new Guna.UI2.WinForms.Guna2ComboBox
            {
                Location = new Point(16, top + 24),
                Size = new Size(440, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                BorderColor = FieldBg,
                ForeColor = TextColor,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _cmbProxyMode.Items.AddRange(new object[] { "System proxy (default)", "Manual proxy", "No proxy" });
            _cmbProxyMode.SelectedIndex = ProxyModeToIndex(settings.ProxyMode);
            _cmbProxyMode.SelectedIndexChanged += (s, e) => RefreshProxyFieldsEnabled();

            _txtProxyAddress = new Guna.UI2.WinForms.Guna2TextBox
            {
                Location = new Point(16, top + 64),
                Size = new Size(200, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                BorderColor = FieldBg,
                ForeColor = TextColor,
                PlaceholderText = "proxy host:port",
                PlaceholderForeColor = MutedColor,
                Text = settings.ProxyAddress ?? string.Empty
            };

            _txtProxyUser = new Guna.UI2.WinForms.Guna2TextBox
            {
                Location = new Point(224, top + 64),
                Size = new Size(112, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                BorderColor = FieldBg,
                ForeColor = TextColor,
                PlaceholderText = "Username",
                PlaceholderForeColor = MutedColor,
                Text = settings.ProxyUsername ?? string.Empty
            };

            _txtProxyPass = new Guna.UI2.WinForms.Guna2TextBox
            {
                Location = new Point(344, top + 64),
                Size = new Size(112, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                BorderColor = FieldBg,
                ForeColor = TextColor,
                PlaceholderText = "Password",
                PlaceholderForeColor = MutedColor,
                PasswordChar = '*',
                Text = settings.ProxyPassword ?? string.Empty
            };

            Controls.Add(lblHeader);
            Controls.Add(_cmbProxyMode);
            Controls.Add(_txtProxyAddress);
            Controls.Add(_txtProxyUser);
            Controls.Add(_txtProxyPass);

            RefreshProxyFieldsEnabled();
        }

        private void RefreshProxyFieldsEnabled()
        {
            var manual = _cmbProxyMode.SelectedIndex == 1;
            _txtProxyAddress.Enabled = manual;
            _txtProxyUser.Enabled = manual;
            _txtProxyPass.Enabled = manual;
        }

        private static int ProxyModeToIndex(string proxyMode)
        {
            switch (proxyMode)
            {
                case "Manual":
                    return 1;
                case "None":
                    return 2;
                default:
                    return 0;
            }
        }

        private static string ProxyModeFromIndex(int index)
        {
            switch (index)
            {
                case 1:
                    return "Manual";
                case 2:
                    return "None";
                default:
                    return "System";
            }
        }

        public void FocusProvider(string providerKey)
        {
            foreach (var row in _rows)
            {
                if (row.ProviderKey == providerKey)
                {
                    row.TxtKey.Focus();
                    return;
                }
            }
        }

        private ProviderRow BuildRow(int index, string providerKey, string currentValue, string registerUrl, Func<IScanProvider> factory)
        {
            var top = 16 + index * 116;

            var lblName = new Label
            {
                Text = providerKey,
                Location = new Point(16, top),
                Size = new Size(200, 20),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = TextColor,
                BackColor = BackColor
            };

            var txtKey = new Guna.UI2.WinForms.Guna2TextBox
            {
                Location = new Point(16, top + 24),
                Size = new Size(320, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                BorderColor = FieldBg,
                ForeColor = TextColor,
                PlaceholderText = "Paste your API key here",
                PlaceholderForeColor = MutedColor,
                Text = currentValue ?? string.Empty
            };

            var btnRegister = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(344, top + 24),
                Size = new Size(112, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 8.5F),
                Text = "Sign up"
            };
            btnRegister.Click += (s, e) => BrowserLauncher.Open(registerUrl);

            var lblStatus = new Label
            {
                Location = new Point(16, top + 60),
                Size = new Size(320, 18),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = MutedColor,
                BackColor = BackColor,
                Text = string.IsNullOrEmpty(currentValue) ? "No key yet" : "Not verified this session"
            };

            var row = new ProviderRow { ProviderKey = providerKey, TxtKey = txtKey, LblStatus = lblStatus, CreateProvider = factory };

            var btnTest = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(344, top + 58),
                Size = new Size(112, 28),
                BorderRadius = 8,
                FillColor = AmberColor,
                ForeColor = Color.FromArgb(0x2b, 0x2a, 0x28),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Text = "Test"
            };
            btnTest.Click += async (s, e) => await TestKeyAsync(row, btnTest);

            Controls.Add(lblName);
            Controls.Add(txtKey);
            Controls.Add(btnRegister);
            Controls.Add(lblStatus);
            Controls.Add(btnTest);

            return row;
        }

        private void BuildFooter()
        {
            var top = NetworkSectionTop + 112;

            var btnSave = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(232, top),
                Size = new Size(110, 36),
                BorderRadius = 10,
                FillColor = AmberColor,
                ForeColor = Color.FromArgb(0x2b, 0x2a, 0x28),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "Save"
            };
            btnSave.Click += (s, e) => SaveAndClose();

            var btnClose = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(350, top),
                Size = new Size(106, 36),
                BorderRadius = 10,
                FillColor = FieldBg,
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 9.5F),
                Text = "Close"
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(btnSave);
            Controls.Add(btnClose);
        }

        private async Task TestKeyAsync(ProviderRow row, Control triggerButton)
        {
            var key = row.TxtKey.Text.Trim();
            if (string.IsNullOrEmpty(key))
            {
                row.LblStatus.Text = "No key entered";
                row.LblStatus.ForeColor = ErrColor;
                return;
            }

            // save right away when Test is clicked, so the provider reads the key you just typed (no need to click Save first)
            var settings = SettingsStore.Load();
            ApplyRowToSettings(row, settings);
            SettingsStore.Save(settings);

            triggerButton.Enabled = false;
            row.LblStatus.Text = "Testing...";
            row.LblStatus.ForeColor = MutedColor;

            try
            {
                var provider = row.CreateProvider();
                var result = await provider.LookupByHashAsync(ProbeHash, CancellationToken.None);

                if (result != null && result.ErrorMessage != null)
                {
                    row.LblStatus.Text = result.ErrorMessage;
                    row.LblStatus.ForeColor = ErrColor;
                }
                else
                {
                    row.LblStatus.Text = "Key is valid";
                    row.LblStatus.ForeColor = OkColor;

                    // Quota is a best-effort bonus - if it errors or isn't supported, just keep
                    // showing "Key is valid" and don't break the main result.
                    try
                    {
                        var quota = await provider.GetQuotaAsync(CancellationToken.None);
                        if (quota != null)
                        {
                            row.LblStatus.Text = $"Key is valid  •  {quota.Used}/{quota.Allowed} {quota.Label}";
                        }
                    }
                    catch
                    {
                        // ignore, keep showing "Key is valid"
                    }
                }
            }
            catch (MissingApiKeyException)
            {
                row.LblStatus.Text = "No key entered";
                row.LblStatus.ForeColor = ErrColor;
            }
            catch (Exception ex)
            {
                row.LblStatus.Text = "Error: " + ex.Message;
                row.LblStatus.ForeColor = ErrColor;
            }
            finally
            {
                triggerButton.Enabled = true;
            }
        }

        private static void ApplyRowToSettings(ProviderRow row, ApiKeySettings settings)
        {
            switch (row.ProviderKey)
            {
                case "VirusTotal":
                    settings.VirusTotalApiKey = row.TxtKey.Text.Trim();
                    break;
                case "MetaDefender":
                    settings.MetaDefenderApiKey = row.TxtKey.Text.Trim();
                    break;
                case "OpenTIP":
                    settings.OpenTipApiKey = row.TxtKey.Text.Trim();
                    break;
            }
        }

        private void SaveAndClose()
        {
            var settings = SettingsStore.Load();
            foreach (var row in _rows)
            {
                ApplyRowToSettings(row, settings);
            }

            settings.ProxyMode = ProxyModeFromIndex(_cmbProxyMode.SelectedIndex);
            settings.ProxyAddress = _txtProxyAddress.Text.Trim();
            settings.ProxyUsername = _txtProxyUser.Text.Trim();
            settings.ProxyPassword = _txtProxyPass.Text;

            SettingsStore.Save(settings);
            Close();
        }
    }
}
