using System.Drawing;
using System.Windows.Forms;

namespace frm_scan_online.UI
{
    partial class MainBar
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Guna.UI2.WinForms.Guna2Panel pnlPill;
        private Label lblShield;
        private Guna.UI2.WinForms.Guna2TextBox txtFilePath;
        private Guna.UI2.WinForms.Guna2ComboBox cmbSource;
        private Guna.UI2.WinForms.Guna2Button btnScan;
        private Guna.UI2.WinForms.Guna2Panel pnlHashBar;
        private Label lblHashPreview;
        private Guna.UI2.WinForms.Guna2Button btnCopyHash;
        private ToolTip toolTipHash;
        private ContextMenuStrip ctxMenu;
        private ToolStripMenuItem mnuSettings;
        private ToolStripMenuItem mnuVerifyChecksum;
        private ToolStripMenuItem mnuScanUrl;
        private ToolStripMenuItem mnuBatchLookup;
        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem mnuTraySettings;
        private ToolStripMenuItem mnuTrayLockPosition;
        private ToolStripMenuItem mnuTrayStartup;
        private ToolStripMenuItem mnuTrayFeedback;
        private ToolStripMenuItem mnuTrayDonate;
        private ToolStripMenuItem mnuTrayHistory;
        private ToolStripMenuItem mnuTrayCheckUpdate;
        private ToolStripMenuItem mnuTrayExit;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlPill = new Guna.UI2.WinForms.Guna2Panel();
            this.lblShield = new Label();
            this.txtFilePath = new Guna.UI2.WinForms.Guna2TextBox();
            this.cmbSource = new Guna.UI2.WinForms.Guna2ComboBox();
            this.btnScan = new Guna.UI2.WinForms.Guna2Button();
            this.pnlHashBar = new Guna.UI2.WinForms.Guna2Panel();
            this.lblHashPreview = new Label();
            this.btnCopyHash = new Guna.UI2.WinForms.Guna2Button();
            this.toolTipHash = new ToolTip(this.components);
            this.ctxMenu = new ContextMenuStrip(this.components);
            this.mnuSettings = new ToolStripMenuItem();
            this.mnuVerifyChecksum = new ToolStripMenuItem();
            this.mnuScanUrl = new ToolStripMenuItem();
            this.mnuBatchLookup = new ToolStripMenuItem();
            this.trayIcon = new NotifyIcon(this.components);
            this.trayMenu = new ContextMenuStrip(this.components);
            this.mnuTraySettings = new ToolStripMenuItem();
            this.mnuTrayLockPosition = new ToolStripMenuItem();
            this.mnuTrayStartup = new ToolStripMenuItem();
            this.mnuTrayFeedback = new ToolStripMenuItem();
            this.mnuTrayDonate = new ToolStripMenuItem();
            this.mnuTrayHistory = new ToolStripMenuItem();
            this.mnuTrayCheckUpdate = new ToolStripMenuItem();
            this.mnuTrayExit = new ToolStripMenuItem();

            this.SuspendLayout();

            // MainBar
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(574, 92);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "MainBar";
            this.Text = "File Scan Tool";
            this.StartPosition = FormStartPosition.Manual;
            this.TopMost = true;
            this.KeyPreview = true;
            this.BackColor = Color.Magenta;
            this.TransparencyKey = Color.Magenta;
            this.Load += new System.EventHandler(this.MainBar_Load);
            this.KeyDown += new KeyEventHandler(this.MainBar_KeyDown);
            this.FormClosing += new FormClosingEventHandler(this.MainBar_FormClosing);

            // pnlPill
            this.pnlPill.Location = new Point(0, 0);
            this.pnlPill.Size = new Size(574, 64);
            this.pnlPill.BorderRadius = 32;
            this.pnlPill.FillColor = Color.FromArgb(0x2b, 0x2a, 0x28);
            this.pnlPill.Name = "pnlPill";
            this.pnlPill.AllowDrop = true;
            this.pnlPill.MouseDown += new MouseEventHandler(this.DragArea_MouseDown);
            this.pnlPill.DragEnter += new DragEventHandler(this.DragArea_DragEnter);
            this.pnlPill.DragDrop += new DragEventHandler(this.DragArea_DragDrop);

            // lblShield
            this.lblShield.Text = "\U0001F6E1";
            this.lblShield.Font = new Font("Segoe UI Emoji", 14F, FontStyle.Regular);
            this.lblShield.ForeColor = Color.FromArgb(0xd9, 0xd7, 0xd2);
            this.lblShield.BackColor = Color.FromArgb(0x2b, 0x2a, 0x28);
            this.lblShield.TextAlign = ContentAlignment.MiddleCenter;
            this.lblShield.Location = new Point(16, 16);
            this.lblShield.Size = new Size(30, 30);
            this.lblShield.Name = "lblShield";
            this.lblShield.ContextMenuStrip = this.ctxMenu;
            this.lblShield.AllowDrop = true;
            this.lblShield.MouseDown += new MouseEventHandler(this.DragArea_MouseDown);
            this.lblShield.DragEnter += new DragEventHandler(this.DragArea_DragEnter);
            this.lblShield.DragDrop += new DragEventHandler(this.DragArea_DragDrop);

            // txtFilePath
            this.txtFilePath.Location = new Point(54, 13);
            this.txtFilePath.Size = new Size(260, 38);
            this.txtFilePath.BorderRadius = 18;
            this.txtFilePath.FillColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.txtFilePath.ForeColor = Color.White;
            this.txtFilePath.BorderColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.txtFilePath.PlaceholderText = "Select a file to scan...";
            this.txtFilePath.PlaceholderForeColor = Color.Gainsboro;
            this.txtFilePath.ReadOnly = true;
            this.txtFilePath.Cursor = Cursors.Hand;
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.HoverState.FillColor = Color.FromArgb(0x45, 0x43, 0x40);
            this.txtFilePath.HoverState.BorderColor = Color.FromArgb(0x45, 0x43, 0x40);
            this.txtFilePath.AllowDrop = true;
            this.txtFilePath.Click += new System.EventHandler(this.txtFilePath_Click);
            this.txtFilePath.DragEnter += new DragEventHandler(this.DragArea_DragEnter);
            this.txtFilePath.DragDrop += new DragEventHandler(this.DragArea_DragDrop);

            // cmbSource
            this.cmbSource.Location = new Point(324, 13);
            this.cmbSource.Size = new Size(120, 38);
            this.cmbSource.BorderRadius = 18;
            this.cmbSource.FillColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.cmbSource.ForeColor = Color.White;
            this.cmbSource.BorderColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.cmbSource.DropDownStyle = ComboBoxStyle.DropDownList;
            this.cmbSource.Items.AddRange(new object[] { "VirusTotal", "MetaDefender", "OpenTIP" });
            this.cmbSource.SelectedIndex = 0;
            this.cmbSource.Name = "cmbSource";
            this.cmbSource.HoverState.FillColor = Color.FromArgb(0x45, 0x43, 0x40);
            this.cmbSource.HoverState.BorderColor = Color.FromArgb(0x45, 0x43, 0x40);
            this.cmbSource.SelectedIndexChanged += new System.EventHandler(this.cmbSource_SelectedIndexChanged);

            // btnScan
            this.btnScan.Location = new Point(454, 10);
            this.btnScan.Size = new Size(100, 44);
            this.btnScan.BorderRadius = 22;
            this.btnScan.FillColor = Color.FromArgb(0xd8, 0x5a, 0x30);
            this.btnScan.ForeColor = Color.FromArgb(0xfb, 0xe9, 0xe2);
            this.btnScan.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnScan.Text = "Scan";
            this.btnScan.Name = "btnScan";
            this.btnScan.HoverState.FillColor = Color.FromArgb(0xe8, 0x73, 0x4a);
            this.btnScan.HoverState.ForeColor = Color.White;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);

            // pnlHashBar
            this.pnlHashBar.Location = new Point(0, 68);
            this.pnlHashBar.Size = new Size(574, 24);
            this.pnlHashBar.BorderRadius = 10;
            this.pnlHashBar.FillColor = Color.FromArgb(0x2b, 0x2a, 0x28);
            this.pnlHashBar.Visible = false;
            this.pnlHashBar.Name = "pnlHashBar";
            this.pnlHashBar.AllowDrop = true;
            this.pnlHashBar.MouseDown += new MouseEventHandler(this.DragArea_MouseDown);
            this.pnlHashBar.DragEnter += new DragEventHandler(this.DragArea_DragEnter);
            this.pnlHashBar.DragDrop += new DragEventHandler(this.DragArea_DragDrop);

            // btnCopyHash
            this.btnCopyHash.Dock = DockStyle.Right;
            this.btnCopyHash.Width = 52;
            this.btnCopyHash.BorderRadius = 8;
            this.btnCopyHash.FillColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.btnCopyHash.ForeColor = Color.FromArgb(0xcf, 0xce, 0xcb);
            this.btnCopyHash.Font = new Font("Segoe UI", 7.5F, FontStyle.Regular);
            this.btnCopyHash.Text = "Copy";
            this.btnCopyHash.Name = "btnCopyHash";
            this.btnCopyHash.HoverState.FillColor = Color.FromArgb(0x45, 0x43, 0x40);
            this.btnCopyHash.Click += new System.EventHandler(this.btnCopyHash_Click);

            // lblHashPreview
            this.lblHashPreview.Dock = DockStyle.Fill;
            this.lblHashPreview.Font = new Font("Consolas", 8.5F, FontStyle.Regular);
            this.lblHashPreview.ForeColor = Color.FromArgb(0x9a, 0x99, 0x96);
            this.lblHashPreview.BackColor = Color.FromArgb(0x2b, 0x2a, 0x28);
            this.lblHashPreview.TextAlign = ContentAlignment.MiddleLeft;
            this.lblHashPreview.Padding = new Padding(14, 0, 0, 0);
            this.lblHashPreview.AutoEllipsis = true;
            this.lblHashPreview.Text = string.Empty;
            this.lblHashPreview.Name = "lblHashPreview";
            this.lblHashPreview.AllowDrop = true;
            this.lblHashPreview.MouseDown += new MouseEventHandler(this.DragArea_MouseDown);
            this.lblHashPreview.DragEnter += new DragEventHandler(this.DragArea_DragEnter);
            this.lblHashPreview.DragDrop += new DragEventHandler(this.DragArea_DragDrop);

            // toolTipHash
            this.toolTipHash.AutoPopDelay = 15000;
            this.toolTipHash.InitialDelay = 300;
            this.toolTipHash.ReshowDelay = 100;

            // mnuSettings
            this.mnuSettings.Text = "API key settings...";
            this.mnuSettings.Click += new System.EventHandler(this.mnuSettings_Click);

            // mnuVerifyChecksum
            this.mnuVerifyChecksum.Text = "Verify checksum...";
            this.mnuVerifyChecksum.Click += new System.EventHandler(this.mnuVerifyChecksum_Click);

            // mnuScanUrl
            this.mnuScanUrl.Text = "Scan a URL...";
            this.mnuScanUrl.Click += new System.EventHandler(this.mnuScanUrl_Click);

            // mnuBatchLookup
            this.mnuBatchLookup.Text = "Batch hash lookup...";
            this.mnuBatchLookup.Click += new System.EventHandler(this.mnuBatchLookup_Click);

            // ctxMenu
            // Note: "Lock position" is deliberately NOT in this menu - if the pill is locked/covered
            // at this exact spot, another window could sit on top of it and there'd be no way to
            // click this menu again. That option only lives in the tray menu (always reachable
            // whether the pill is covered or not).
            this.ctxMenu.Items.Add(this.mnuVerifyChecksum);
            this.ctxMenu.Items.Add(this.mnuScanUrl);
            this.ctxMenu.Items.Add(this.mnuBatchLookup);
            this.ctxMenu.Items.Add(this.mnuSettings);
            this.pnlPill.ContextMenuStrip = this.ctxMenu;

            // mnuTraySettings
            this.mnuTraySettings.Text = "API key settings...";
            this.mnuTraySettings.Click += new System.EventHandler(this.mnuSettings_Click);

            // mnuTrayLockPosition
            this.mnuTrayLockPosition.Text = "Lock position";
            this.mnuTrayLockPosition.CheckOnClick = true;
            this.mnuTrayLockPosition.Checked = true;

            // mnuTrayStartup
            this.mnuTrayStartup.Text = "Start with Windows";
            this.mnuTrayStartup.CheckOnClick = true;
            this.mnuTrayStartup.Click += new System.EventHandler(this.mnuTrayStartup_Click);

            // mnuTrayFeedback
            this.mnuTrayFeedback.Text = "Send feedback";
            this.mnuTrayFeedback.Click += new System.EventHandler(this.mnuTrayFeedback_Click);

            // mnuTrayDonate
            this.mnuTrayDonate.Text = "Support the author";
            this.mnuTrayDonate.Click += new System.EventHandler(this.mnuTrayDonate_Click);

            // mnuTrayHistory
            this.mnuTrayHistory.Text = "Scan history";
            this.mnuTrayHistory.Click += new System.EventHandler(this.mnuTrayHistory_Click);

            // mnuTrayCheckUpdate
            this.mnuTrayCheckUpdate.Text = "Check for updates";
            this.mnuTrayCheckUpdate.Click += new System.EventHandler(this.mnuTrayCheckUpdate_Click);

            // mnuTrayExit
            this.mnuTrayExit.Text = "Exit";
            this.mnuTrayExit.Click += new System.EventHandler(this.mnuTrayExit_Click);

            // trayMenu
            this.trayMenu.Items.Add(this.mnuTraySettings);
            this.trayMenu.Items.Add(this.mnuTrayLockPosition);
            this.trayMenu.Items.Add(this.mnuTrayStartup);
            this.trayMenu.Items.Add(new ToolStripSeparator());
            this.trayMenu.Items.Add(this.mnuTrayHistory);
            this.trayMenu.Items.Add(this.mnuTrayFeedback);
            this.trayMenu.Items.Add(this.mnuTrayDonate);
            this.trayMenu.Items.Add(this.mnuTrayCheckUpdate);
            this.trayMenu.Items.Add(new ToolStripSeparator());
            this.trayMenu.Items.Add(this.mnuTrayExit);
            this.trayMenu.Opening += new System.ComponentModel.CancelEventHandler(this.trayMenu_Opening);

            // trayIcon
            this.trayIcon.Icon = SystemIcons.Shield;
            this.trayIcon.Text = "File Scan Tool";
            this.trayIcon.ContextMenuStrip = this.trayMenu;
            this.trayIcon.Visible = true;
            this.trayIcon.MouseClick += new MouseEventHandler(this.trayIcon_MouseClick);
            this.trayIcon.DoubleClick += new System.EventHandler(this.trayIcon_DoubleClick);
            this.trayIcon.BalloonTipClicked += new System.EventHandler(this.trayIcon_BalloonTipClicked);

            // pnlPill children
            this.pnlPill.Controls.Add(this.lblShield);
            this.pnlPill.Controls.Add(this.txtFilePath);
            this.pnlPill.Controls.Add(this.cmbSource);
            this.pnlPill.Controls.Add(this.btnScan);

            // pnlHashBar children
            this.pnlHashBar.Controls.Add(this.lblHashPreview);
            this.pnlHashBar.Controls.Add(this.btnCopyHash);

            this.Controls.Add(this.pnlHashBar);
            this.Controls.Add(this.pnlPill);

            this.ResumeLayout(false);
        }

        #endregion
    }
}
