using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using frm_scan_online.Models;

namespace frm_scan_online.UI
{
    public partial class ChecksumForm : Form
    {
        private static readonly Color TextColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
        private static readonly Color MutedColor = Color.FromArgb(0x8a, 0x89, 0x86);
        private static readonly Color OkColor = Color.FromArgb(0x4c, 0xaf, 0x50);
        private static readonly Color ErrColor = Color.FromArgb(0xef, 0x53, 0x50);
        private static readonly Color FieldBg = Color.FromArgb(0x3a, 0x39, 0x36);
        private static readonly Color AccentColor = Color.FromArgb(0xd8, 0x5a, 0x30);

        private readonly FileHashInfo _hash;
        private readonly Guna.UI2.WinForms.Guna2TextBox _txtExpected;
        private readonly Label _lblResult;

        public ChecksumForm(string fileName, FileHashInfo hash)
        {
            _hash = hash;
            InitializeComponent();

            var lblFileName = new Label
            {
                Text = "File: " + fileName,
                Location = new Point(16, 16),
                Size = new Size(428, 20),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = TextColor,
                BackColor = BackColor,
                AutoEllipsis = true
            };

            var lblInstruction = new Label
            {
                Text = "Paste the expected checksum (MD5, SHA-1 or SHA-256) shown on the download page:",
                Location = new Point(16, 44),
                Size = new Size(428, 36),
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = MutedColor,
                BackColor = BackColor
            };

            _txtExpected = new Guna.UI2.WinForms.Guna2TextBox
            {
                Location = new Point(16, 86),
                Size = new Size(428, 34),
                BorderRadius = 8,
                FillColor = FieldBg,
                BorderColor = FieldBg,
                ForeColor = TextColor,
                PlaceholderText = "e.g. a1b2c3d4...",
                PlaceholderForeColor = MutedColor
            };

            var btnVerify = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(16, 130),
                Size = new Size(120, 34),
                BorderRadius = 8,
                FillColor = AccentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Text = "Verify"
            };
            btnVerify.Click += (s, e) => Verify();

            _lblResult = new Label
            {
                Location = new Point(150, 130),
                Size = new Size(294, 60),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = MutedColor,
                BackColor = BackColor,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var btnClose = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(350, 180),
                Size = new Size(94, 32),
                BorderRadius = 8,
                FillColor = FieldBg,
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 9F),
                Text = "Close"
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(lblFileName);
            Controls.Add(lblInstruction);
            Controls.Add(_txtExpected);
            Controls.Add(btnVerify);
            Controls.Add(_lblResult);
            Controls.Add(btnClose);
        }

        private void Verify()
        {
            var input = _txtExpected.Text.Trim().Replace(" ", string.Empty);

            string actual;
            switch (input.Length)
            {
                case 32:
                    actual = _hash.Md5;
                    break;
                case 40:
                    actual = _hash.Sha1;
                    break;
                case 64:
                    actual = _hash.Sha256;
                    break;
                default:
                    _lblResult.Text = "Unrecognized length - expected 32 (MD5), 40 (SHA-1) or 64 (SHA-256) hex characters.";
                    _lblResult.ForeColor = MutedColor;
                    return;
            }

            if (string.Equals(input, actual, StringComparison.OrdinalIgnoreCase))
            {
                _lblResult.Text = "Match - the file is intact.";
                _lblResult.ForeColor = OkColor;
            }
            else
            {
                _lblResult.Text = "MISMATCH - the file may be corrupted or tampered with.";
                _lblResult.ForeColor = ErrColor;
            }
        }
    }
}
