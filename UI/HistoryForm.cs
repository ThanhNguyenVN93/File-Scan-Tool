using System;
using System.Windows.Forms;
using frm_scan_online.Services;

namespace frm_scan_online.UI
{
    public partial class HistoryForm : Form
    {
        public HistoryForm()
        {
            InitializeComponent();
            LoadHistory();
        }

        private void LoadHistory()
        {
            lvHistory.Items.Clear();

            foreach (var entry in ScanHistoryStore.Load())
            {
                var item = new ListViewItem(entry.Timestamp.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add(entry.FileName);
                item.SubItems.Add(entry.SourceName);
                item.SubItems.Add(entry.IsClean ? "Clean" : "Infected (" + entry.DetectionRatio + ")");
                item.SubItems.Add(entry.Sha256);
                item.ForeColor = entry.IsClean
                    ? System.Drawing.Color.FromArgb(0xe8, 0xe6, 0xe2)
                    : System.Drawing.Color.FromArgb(0xef, 0x53, 0x50);

                lvHistory.Items.Add(item);
            }
        }

        private void btnClearHistory_Click(object sender, EventArgs e)
        {
            var confirm = MessageBox.Show(this, "Clear the entire scan history?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                ScanHistoryStore.Clear();
                LoadHistory();
            }
        }

        private void btnCloseHistory_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
