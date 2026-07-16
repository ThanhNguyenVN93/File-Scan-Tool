using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using frm_scan_online.Services;

namespace frm_scan_online.UI
{
    public partial class DonateForm : Form
    {
        private const string KofiUrl = "https://ko-fi.com/thanhnguyen150993";

        private static readonly Color TextColor = Color.FromArgb(0xe8, 0xe6, 0xe2);
        private static readonly Color MutedColor = Color.FromArgb(0x8a, 0x89, 0x86);
        private static readonly Color FieldBg = Color.FromArgb(0x3a, 0x39, 0x36);
        private static readonly Color AccentColor = Color.FromArgb(0xd8, 0x5a, 0x30);

        public DonateForm()
        {
            InitializeComponent();
            BuildContent();
        }

        private void BuildContent()
        {
            var lblTitle = new Label
            {
                Text = "Thanks for using File Scan Tool!",
                Location = new Point(16, 16),
                Size = new Size(428, 22),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = TextColor,
                BackColor = BackColor,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var picTcb = new PictureBox
            {
                Location = new Point(30, 54),
                Size = new Size(180, 180),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = FieldBg,
                Image = LoadEmbeddedImage("tcb.jpg")
            };

            var lblTcb = new Label
            {
                Text = "Techcombank",
                Location = new Point(30, 238),
                Size = new Size(180, 18),
                ForeColor = MutedColor,
                BackColor = BackColor,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var picMomo = new PictureBox
            {
                Location = new Point(250, 54),
                Size = new Size(180, 180),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = FieldBg,
                Image = LoadEmbeddedImage("momo.jpg")
            };

            var lblMomo = new Label
            {
                Text = "MoMo",
                Location = new Point(250, 238),
                Size = new Size(180, 18),
                ForeColor = MutedColor,
                BackColor = BackColor,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnKofi = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(110, 274),
                Size = new Size(240, 38),
                BorderRadius = 10,
                FillColor = AccentColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "Support via Ko-fi"
            };
            btnKofi.Click += (s, e) => BrowserLauncher.Open(KofiUrl);

            var btnClose = new Guna.UI2.WinForms.Guna2Button
            {
                Location = new Point(180, 324),
                Size = new Size(100, 32),
                BorderRadius = 10,
                FillColor = FieldBg,
                ForeColor = TextColor,
                Font = new Font("Segoe UI", 9F),
                Text = "Close"
            };
            btnClose.Click += (s, e) => Close();

            Controls.Add(lblTitle);
            Controls.Add(picTcb);
            Controls.Add(lblTcb);
            Controls.Add(picMomo);
            Controls.Add(lblMomo);
            Controls.Add(btnKofi);
            Controls.Add(btnClose);
        }

        private static Image LoadEmbeddedImage(string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "frm_scan_online.Assets." + fileName;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                return stream != null ? Image.FromStream(stream) : null;
            }
        }
    }
}
