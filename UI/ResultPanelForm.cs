using System.Windows.Forms;

namespace frm_scan_online.UI
{
    public partial class ResultPanelForm : Form
    {
        public ResultPanelForm()
        {
            InitializeComponent();
        }

        public void ShowBelow(Form owner)
        {
            Location = new System.Drawing.Point(owner.Left, owner.Bottom + 6);
            if (!Visible)
            {
                Show(owner);
            }
        }
    }
}
