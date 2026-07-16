using System.Drawing;
using System.Windows.Forms;

namespace frm_scan_online.UI
{
    partial class BatchLookupForm
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
            this.ClientSize = new Size(512, 470);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Batch hash lookup";
            this.BackColor = Color.FromArgb(0x21, 0x1f, 0x1d);
            this.Font = new Font("Segoe UI", 9F);
            this.Name = "BatchLookupForm";

            this.ResumeLayout(false);
        }
    }
}
