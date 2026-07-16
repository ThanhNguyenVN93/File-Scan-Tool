using System.Drawing;
using System.Windows.Forms;

namespace frm_scan_online.UI
{
    partial class ChecksumForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.SuspendLayout();

            this.AutoScaleMode = AutoScaleMode.Font;
            this.ClientSize = new Size(460, 220);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Verify checksum";
            this.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.Font = new Font("Segoe UI", 9F);
            this.Name = "ChecksumForm";

            this.ResumeLayout(false);
        }
    }
}
