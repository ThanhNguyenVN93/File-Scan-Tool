using System.Drawing;
using System.Windows.Forms;

namespace frm_scan_online.UI
{
    partial class ResultPanelForm
    {
        private System.ComponentModel.IContainer components = null;

        public ResultPanel Panel { get; private set; }

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
            this.Panel = new ResultPanel();

            this.SuspendLayout();

            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(574, 360);
            this.FormBorderStyle = FormBorderStyle.None;
            this.Name = "ResultPanelForm";
            this.Text = "File Scan Tool - Result";
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;
            // Not using TransparencyKey here: nested controls (label/panel/listview) can easily get
            // "hidden" by the transparent color, causing rendering glitches. Use a color matching
            // pnlRoot.FillColor instead.
            this.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);

            this.Panel.Dock = DockStyle.Fill;
            this.Controls.Add(this.Panel);

            this.ResumeLayout(false);
        }
    }
}
