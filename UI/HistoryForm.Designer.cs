using System.Drawing;
using System.Windows.Forms;

namespace frm_scan_online.UI
{
    partial class HistoryForm
    {
        private System.ComponentModel.IContainer components = null;

        private ListView lvHistory;
        private ColumnHeader colTime;
        private ColumnHeader colFile;
        private ColumnHeader colSource;
        private ColumnHeader colResult;
        private ColumnHeader colHash;
        private Guna.UI2.WinForms.Guna2Button btnClearHistory;
        private Guna.UI2.WinForms.Guna2Button btnCloseHistory;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();

            this.lvHistory = new ListView();
            this.colTime = new ColumnHeader();
            this.colFile = new ColumnHeader();
            this.colSource = new ColumnHeader();
            this.colResult = new ColumnHeader();
            this.colHash = new ColumnHeader();
            this.btnClearHistory = new Guna.UI2.WinForms.Guna2Button();
            this.btnCloseHistory = new Guna.UI2.WinForms.Guna2Button();

            this.SuspendLayout();

            // HistoryForm
            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(700, 420);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimumSize = new Size(500, 300);
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Scan history";
            this.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.Font = new Font("Segoe UI", 9F);
            this.Name = "HistoryForm";

            // lvHistory
            this.lvHistory.Location = new Point(16, 16);
            this.lvHistory.Size = new Size(668, 348);
            this.lvHistory.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            this.lvHistory.View = View.Details;
            this.lvHistory.FullRowSelect = true;
            this.lvHistory.GridLines = false;
            this.lvHistory.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            this.lvHistory.BackColor = Color.FromArgb(0x2b, 0x2a, 0x28);
            this.lvHistory.ForeColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
            this.lvHistory.BorderStyle = BorderStyle.None;
            this.lvHistory.Name = "lvHistory";

            this.colTime.Text = "Time";
            this.colTime.Width = 130;
            this.colFile.Text = "File";
            this.colFile.Width = 160;
            this.colSource.Text = "Source";
            this.colSource.Width = 90;
            this.colResult.Text = "Result";
            this.colResult.Width = 110;
            this.colHash.Text = "SHA-256";
            this.colHash.Width = 170;

            this.lvHistory.Columns.AddRange(new ColumnHeader[]
            {
                this.colTime, this.colFile, this.colSource, this.colResult, this.colHash
            });

            // btnClearHistory
            this.btnClearHistory.Location = new Point(16, 376);
            this.btnClearHistory.Size = new Size(140, 32);
            this.btnClearHistory.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            this.btnClearHistory.BorderRadius = 8;
            this.btnClearHistory.FillColor = Color.FromArgb(0x3a, 0x39, 0x36);
            this.btnClearHistory.ForeColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
            this.btnClearHistory.Font = new Font("Segoe UI", 9F);
            this.btnClearHistory.Text = "Clear history";
            this.btnClearHistory.Name = "btnClearHistory";
            this.btnClearHistory.Click += new System.EventHandler(this.btnClearHistory_Click);

            // btnCloseHistory
            this.btnCloseHistory.Location = new Point(584, 376);
            this.btnCloseHistory.Size = new Size(100, 32);
            this.btnCloseHistory.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            this.btnCloseHistory.BorderRadius = 8;
            this.btnCloseHistory.FillColor = Color.FromArgb(0xd8, 0x5a, 0x30);
            this.btnCloseHistory.ForeColor = Color.White;
            this.btnCloseHistory.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnCloseHistory.Text = "Close";
            this.btnCloseHistory.Name = "btnCloseHistory";
            this.btnCloseHistory.Click += new System.EventHandler(this.btnCloseHistory_Click);

            this.Controls.Add(this.lvHistory);
            this.Controls.Add(this.btnClearHistory);
            this.Controls.Add(this.btnCloseHistory);

            this.ResumeLayout(false);
        }
    }
}
