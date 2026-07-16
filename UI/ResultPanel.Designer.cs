using System.Drawing;
using System.Windows.Forms;

namespace frm_scan_online.UI
{
    partial class ResultPanel
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        private Guna.UI2.WinForms.Guna2Panel pnlRoot;
        private Label lblStatus;
        private Guna.UI2.WinForms.Guna2Button btnRescan;
        private Guna.UI2.WinForms.Guna2ProgressBar pgbScan;
        private Label lblTabInfected;
        private Label lblTabClean;
        private Panel pnlInfectedContent;
        private ListView lvEngines;
        private ColumnHeader colEngine;
        private ColumnHeader colVerdict;
        private Panel pnlInfectedFooter;
        private Guna.UI2.WinForms.Guna2Button btnDelete;
        private Guna.UI2.WinForms.Guna2Button btnQuarantine;
        private Label lblActionStatus;
        private Panel pnlCleanContent;
        private Label lblCleanSummary;
        private Label lblErrorMessage;
        private Label lblHashSha256;
        private Label lblHashMd5;
        private Guna.UI2.WinForms.Guna2Button btnExport;
        private Guna.UI2.WinForms.Guna2Button btnViewSource;

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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.pnlRoot = new Guna.UI2.WinForms.Guna2Panel();
            this.lblStatus = new Label();
            this.btnRescan = new Guna.UI2.WinForms.Guna2Button();
            this.pgbScan = new Guna.UI2.WinForms.Guna2ProgressBar();
            this.lblTabInfected = new Label();
            this.lblTabClean = new Label();
            this.pnlInfectedContent = new Panel();
            this.lvEngines = new ListView();
            this.colEngine = new ColumnHeader();
            this.colVerdict = new ColumnHeader();
            this.pnlInfectedFooter = new Panel();
            this.btnDelete = new Guna.UI2.WinForms.Guna2Button();
            this.btnQuarantine = new Guna.UI2.WinForms.Guna2Button();
            this.lblActionStatus = new Label();
            this.pnlCleanContent = new Panel();
            this.lblCleanSummary = new Label();
            this.lblErrorMessage = new Label();
            this.lblHashSha256 = new Label();
            this.lblHashMd5 = new Label();
            this.btnExport = new Guna.UI2.WinForms.Guna2Button();
            this.btnViewSource = new Guna.UI2.WinForms.Guna2Button();

            this.SuspendLayout();

            // ResultPanel
            this.Size = new Size(574, 360);
            this.Name = "ResultPanel";

            // pnlRoot
            this.pnlRoot.Dock = DockStyle.Fill;
            this.pnlRoot.BorderRadius = 12;
            this.pnlRoot.BorderThickness = 1;
            this.pnlRoot.BorderColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.pnlRoot.FillColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.pnlRoot.Name = "pnlRoot";

            // lblStatus
            this.lblStatus.Location = new Point(16, 12);
            this.lblStatus.Size = new Size(410, 18);
            this.lblStatus.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.lblStatus.ForeColor = Color.FromArgb(0xcf, 0xce, 0xcb);
            this.lblStatus.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.lblStatus.AutoEllipsis = true;
            this.lblStatus.Name = "lblStatus";

            // btnRescan
            this.btnRescan.Location = new Point(434, 6);
            this.btnRescan.Size = new Size(124, 28);
            this.btnRescan.BorderRadius = 8;
            this.btnRescan.FillColor = Color.FromArgb(0xd8, 0x5a, 0x30);
            this.btnRescan.ForeColor = Color.White;
            this.btnRescan.Font = new Font("Segoe UI", 8F, FontStyle.Bold);
            this.btnRescan.Text = "Rescan";
            this.btnRescan.Visible = false;
            this.btnRescan.Name = "btnRescan";
            this.btnRescan.Click += new System.EventHandler(this.btnRescan_Click);

            // pgbScan
            this.pgbScan.Location = new Point(16, 32);
            this.pgbScan.Size = new Size(542, 8);
            this.pgbScan.Minimum = 0;
            this.pgbScan.Maximum = 100;
            this.pgbScan.Value = 0;
            this.pgbScan.ProgressColor = Color.FromArgb(0xf2, 0xa6, 0x23);
            this.pgbScan.ProgressColor2 = Color.FromArgb(0xf2, 0xa6, 0x23);
            this.pgbScan.Name = "pgbScan";

            // lblTabInfected
            this.lblTabInfected.Location = new Point(16, 52);
            this.lblTabInfected.Size = new Size(180, 24);
            this.lblTabInfected.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            this.lblTabInfected.ForeColor = Color.FromArgb(0xef, 0x53, 0x50);
            this.lblTabInfected.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.lblTabInfected.Cursor = Cursors.Hand;
            this.lblTabInfected.Text = "Infected (0)";
            this.lblTabInfected.Name = "lblTabInfected";
            this.lblTabInfected.Click += new System.EventHandler(this.lblTabInfected_Click);

            // lblTabClean
            this.lblTabClean.Location = new Point(200, 52);
            this.lblTabClean.Size = new Size(140, 24);
            this.lblTabClean.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.lblTabClean.ForeColor = Color.FromArgb(0x8a, 0x89, 0x86);
            this.lblTabClean.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.lblTabClean.Cursor = Cursors.Hand;
            this.lblTabClean.Text = "Clean (0)";
            this.lblTabClean.Name = "lblTabClean";
            this.lblTabClean.Click += new System.EventHandler(this.lblTabClean_Click);

            // lvEngines
            this.lvEngines.Dock = DockStyle.Fill;
            this.lvEngines.View = View.Details;
            this.lvEngines.FullRowSelect = true;
            this.lvEngines.GridLines = false;
            this.lvEngines.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.lvEngines.Font = new Font("Segoe UI", 9F, FontStyle.Regular);
            this.lvEngines.BackColor = Color.FromArgb(0x2b, 0x2a, 0x28);
            this.lvEngines.ForeColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
            this.lvEngines.BorderStyle = BorderStyle.None;
            this.lvEngines.Name = "lvEngines";

            // colEngine
            this.colEngine.Text = "Engine";
            this.colEngine.Width = 220;

            // colVerdict
            this.colVerdict.Text = "Result";
            this.colVerdict.Width = 300;

            this.lvEngines.Columns.AddRange(new ColumnHeader[] { this.colEngine, this.colVerdict });

            // btnDelete
            this.btnDelete.Location = new Point(0, 4);
            this.btnDelete.Size = new Size(110, 30);
            this.btnDelete.BorderRadius = 8;
            this.btnDelete.FillColor = Color.FromArgb(0xef, 0x53, 0x50);
            this.btnDelete.ForeColor = Color.White;
            this.btnDelete.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.btnDelete.Text = "Delete file";
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);

            // btnQuarantine
            this.btnQuarantine.Location = new Point(120, 4);
            this.btnQuarantine.Size = new Size(140, 30);
            this.btnQuarantine.BorderRadius = 8;
            this.btnQuarantine.FillColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.btnQuarantine.ForeColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
            this.btnQuarantine.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            this.btnQuarantine.Text = "Quarantine file";
            this.btnQuarantine.Name = "btnQuarantine";
            this.btnQuarantine.Click += new System.EventHandler(this.btnQuarantine_Click);

            // lblActionStatus
            this.lblActionStatus.Location = new Point(270, 4);
            this.lblActionStatus.Size = new Size(272, 30);
            this.lblActionStatus.Font = new Font("Segoe UI", 8.5F, FontStyle.Regular);
            this.lblActionStatus.ForeColor = Color.FromArgb(0x8a, 0x89, 0x86);
            this.lblActionStatus.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.lblActionStatus.TextAlign = ContentAlignment.MiddleLeft;
            this.lblActionStatus.Name = "lblActionStatus";

            // pnlInfectedFooter
            this.pnlInfectedFooter.Dock = DockStyle.Bottom;
            this.pnlInfectedFooter.Height = 38;
            this.pnlInfectedFooter.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.pnlInfectedFooter.Name = "pnlInfectedFooter";
            this.pnlInfectedFooter.Controls.Add(this.lblActionStatus);
            this.pnlInfectedFooter.Controls.Add(this.btnQuarantine);
            this.pnlInfectedFooter.Controls.Add(this.btnDelete);

            // pnlInfectedContent
            this.pnlInfectedContent.Location = new Point(16, 84);
            this.pnlInfectedContent.Size = new Size(542, 220);
            this.pnlInfectedContent.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.pnlInfectedContent.Name = "pnlInfectedContent";
            this.pnlInfectedContent.Controls.Add(this.lvEngines);
            this.pnlInfectedContent.Controls.Add(this.pnlInfectedFooter);

            // lblCleanSummary
            this.lblCleanSummary.Dock = DockStyle.Top;
            this.lblCleanSummary.Font = new Font("Segoe UI", 10F, FontStyle.Regular);
            this.lblCleanSummary.ForeColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
            this.lblCleanSummary.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.lblCleanSummary.TextAlign = ContentAlignment.TopLeft;
            this.lblCleanSummary.Height = 40;
            this.lblCleanSummary.Name = "lblCleanSummary";

            // pnlCleanContent
            this.pnlCleanContent.Location = new Point(16, 84);
            this.pnlCleanContent.Size = new Size(542, 220);
            this.pnlCleanContent.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.pnlCleanContent.Visible = false;
            this.pnlCleanContent.Name = "pnlCleanContent";
            this.pnlCleanContent.Controls.Add(this.lblCleanSummary);

            // lblErrorMessage
            this.lblErrorMessage.Location = new Point(16, 84);
            this.lblErrorMessage.Size = new Size(542, 220);
            this.lblErrorMessage.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
            this.lblErrorMessage.ForeColor = Color.FromArgb(0xef, 0x53, 0x50);
            this.lblErrorMessage.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.lblErrorMessage.TextAlign = ContentAlignment.TopLeft;
            this.lblErrorMessage.Visible = false;
            this.lblErrorMessage.Name = "lblErrorMessage";

            // lblHashSha256
            this.lblHashSha256.Location = new Point(16, 314);
            this.lblHashSha256.Size = new Size(296, 16);
            this.lblHashSha256.Font = new Font("Consolas", 8.5F, FontStyle.Regular);
            this.lblHashSha256.ForeColor = Color.FromArgb(0x8a, 0x89, 0x86);
            this.lblHashSha256.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.lblHashSha256.AutoEllipsis = true;
            this.lblHashSha256.Name = "lblHashSha256";

            // lblHashMd5
            this.lblHashMd5.Location = new Point(16, 332);
            this.lblHashMd5.Size = new Size(296, 16);
            this.lblHashMd5.Font = new Font("Consolas", 8.5F, FontStyle.Regular);
            this.lblHashMd5.ForeColor = Color.FromArgb(0x8a, 0x89, 0x86);
            this.lblHashMd5.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.lblHashMd5.AutoEllipsis = true;
            this.lblHashMd5.Name = "lblHashMd5";

            // btnExport
            this.btnExport.Location = new Point(320, 312);
            this.btnExport.Size = new Size(100, 36);
            this.btnExport.BorderRadius = 8;
            this.btnExport.FillColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.btnExport.ForeColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
            this.btnExport.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            this.btnExport.Text = "Export";
            this.btnExport.Visible = false;
            this.btnExport.Name = "btnExport";
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);

            // btnViewSource
            this.btnViewSource.Location = new Point(428, 312);
            this.btnViewSource.Size = new Size(130, 36);
            this.btnViewSource.BorderRadius = 8;
            this.btnViewSource.FillColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.btnViewSource.ForeColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
            this.btnViewSource.Font = new Font("Segoe UI", 8F, FontStyle.Regular);
            this.btnViewSource.Text = "View details";
            this.btnViewSource.Visible = false;
            this.btnViewSource.Name = "btnViewSource";
            this.btnViewSource.Click += new System.EventHandler(this.btnViewSource_Click);

            // pnlRoot children
            this.pnlRoot.Controls.Add(this.btnViewSource);
            this.pnlRoot.Controls.Add(this.btnExport);
            this.pnlRoot.Controls.Add(this.lblHashMd5);
            this.pnlRoot.Controls.Add(this.lblHashSha256);
            this.pnlRoot.Controls.Add(this.lblErrorMessage);
            this.pnlRoot.Controls.Add(this.pnlCleanContent);
            this.pnlRoot.Controls.Add(this.pnlInfectedContent);
            this.pnlRoot.Controls.Add(this.lblTabClean);
            this.pnlRoot.Controls.Add(this.lblTabInfected);
            this.pnlRoot.Controls.Add(this.pgbScan);
            this.pnlRoot.Controls.Add(this.btnRescan);
            this.pnlRoot.Controls.Add(this.lblStatus);

            this.Controls.Add(this.pnlRoot);

            this.ResumeLayout(false);
        }

        #endregion
    }
}
