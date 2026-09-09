using System;
using System.Windows.Forms;
using PHARMA.Services;
using PHARMA.UI.Helpers;
using PHARMA.UI.Forms.POS;

namespace PHARMA.UI.Forms
{
    public partial class MainMdiForm : Form
    {
        private readonly AuthService _auth = new AuthService();

        public MainMdiForm()
        {
            InitializeComponent();
            this.IsMdiContainer = true;
            this.WindowState = FormWindowState.Maximized;
            this.KeyPreview = true;

            BuildMenu();
            UpdateStatus();
        }

        private void BuildMenu()
        {
            MenuBuilder.Build(menuStrip1, _auth, OpenModule);

            var saleMenu = new ToolStripMenuItem("&Sale");
            var posItem = new ToolStripMenuItem("&POS / Billing");
            posItem.ShortcutKeys = Keys.Control | Keys.S;
            posItem.Click += (s, e) => OpenModule("POS");
            saleMenu.DropDownItems.Add(posItem);
            menuStrip1.Items.Insert(0, saleMenu);
        }

        private void OpenModule(string key)
        {
            if (string.IsNullOrEmpty(key)) return;

            key = key.ToUpperInvariant();

            if (key.Contains("POS") || key.Contains("SALE") || key.Contains("BILL"))
            {
                OpenChild(new PosForm());
            }
            else
            {
                MessageBox.Show("Module '" + key + "' will be available soon.\n\nCurrently implemented: POS / Sale", "PHARMA", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void OpenChild(Form child)
        {
            foreach (Form f in this.MdiChildren)
            {
                if (f.GetType() == child.GetType())
                {
                    f.Activate();
                    child.Dispose();
                    return;
                }
            }
            child.MdiParent = this;
            child.Show();
        }

        private void UpdateStatus()
        {
            if (AuthService.CurrentUser != null)
            {
                statusLabel.Text = "User: " + AuthService.CurrentUser.UserName + "  |  F2=New  F5=Save  Ctrl+S=POS  Esc=Close";
            }
        }

        private void MainMdiForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                OpenModule("POS");
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                MessageBox.Show("Keyboard Shortcuts:\n\nCtrl+S  = Open POS\nF2      = New\nF5      = Save\nF3      = Search\nF8      = Delete\nF9      = Print\nEsc     = Close form", "Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Handled = true;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
