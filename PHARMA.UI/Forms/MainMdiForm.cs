using System;
using System.Drawing;
using System.Windows.Forms;
using PHARMA.Services;
using PHARMA.UI.Helpers;
using PHARMA.UI.Forms.POS;

namespace PHARMA.UI.Forms
{
    public partial class MainMdiForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        private Panel _welcomePanel;

        public MainMdiForm()
        {
            InitializeComponent();
            this.IsMdiContainer = true;
            this.WindowState = FormWindowState.Maximized;
            this.KeyPreview = true;
            this.BackColor = Color.FromArgb(240, 240, 245);

            try
            {
                BuildMenuSafe();
            }
            catch (Exception ex)
            {
                // Even if DB menu fails, keep working
                MessageBox.Show("Menu load warning: " + ex.Message, "PHARMA", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                BuildHardcodedMenu();
            }

            ShowWelcome();
            UpdateStatus();
        }

        private void BuildMenuSafe()
        {
            menuStrip1.Items.Clear();
            BuildHardcodedMenu();

            // Optional: add DB menus if table has data
            try
            {
                MenuBuilder.Build(menuStrip1, _auth, OpenModule);
            }
            catch
            {
                // ignore empty MenuName table
            }
        }

        private void BuildHardcodedMenu()
        {
            // File
            var fileMenu = new ToolStripMenuItem("&File");
            var exitItem = new ToolStripMenuItem("E&xit");
            exitItem.ShortcutKeys = Keys.Alt | Keys.F4;
            exitItem.Click += (s, e) => Application.Exit();
            fileMenu.DropDownItems.Add(exitItem);
            menuStrip1.Items.Add(fileMenu);

            // Sale
            var saleMenu = new ToolStripMenuItem("&Sale");
            var posItem = new ToolStripMenuItem("&POS / Billing");
            posItem.ShortcutKeys = Keys.Control | Keys.S;
            posItem.Click += (s, e) => OpenPos();
            saleMenu.DropDownItems.Add(posItem);
            menuStrip1.Items.Add(saleMenu);

            // Help
            var helpMenu = new ToolStripMenuItem("&Help");
            var aboutItem = new ToolStripMenuItem("&Shortcuts (F1)");
            aboutItem.Click += (s, e) => ShowHelp();
            helpMenu.DropDownItems.Add(aboutItem);
            menuStrip1.Items.Add(helpMenu);
        }

        private void ShowWelcome()
        {
            _welcomePanel = new Panel();
            _welcomePanel.Dock = DockStyle.Fill;
            _welcomePanel.BackColor = Color.FromArgb(245, 247, 250);

            var title = new Label();
            title.Text = "PHARMA / PharmaZ";
            title.Font = new Font("Segoe UI", 28F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(30, 60, 120);
            title.AutoSize = true;
            title.Location = new Point(40, 40);

            var user = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "-";
            var subtitle = new Label();
            subtitle.Text = "Welcome, " + user + "\n\nSelect a module from the menu or press a button below.";
            subtitle.Font = new Font("Segoe UI", 12F);
            subtitle.AutoSize = true;
            subtitle.Location = new Point(40, 100);

            var btnPos = new Button();
            btnPos.Text = "POS / Billing  (Ctrl+S)";
            btnPos.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            btnPos.Size = new Size(280, 60);
            btnPos.Location = new Point(40, 180);
            btnPos.BackColor = Color.FromArgb(0, 120, 215);
            btnPos.ForeColor = Color.White;
            btnPos.FlatStyle = FlatStyle.Flat;
            btnPos.Click += (s, e) => OpenPos();

            var btnHelp = new Button();
            btnHelp.Text = "Keyboard Help (F1)";
            btnHelp.Font = new Font("Segoe UI", 12F);
            btnHelp.Size = new Size(280, 45);
            btnHelp.Location = new Point(40, 260);
            btnHelp.Click += (s, e) => ShowHelp();

            var hint = new Label();
            hint.Text = "Menu bar is at the TOP:  File  |  Sale  |  Help\n\nIf you see only this screen, click POS / Billing to start.";
            hint.Font = new Font("Segoe UI", 10F);
            hint.ForeColor = Color.DimGray;
            hint.AutoSize = true;
            hint.Location = new Point(40, 340);

            _welcomePanel.Controls.Add(title);
            _welcomePanel.Controls.Add(subtitle);
            _welcomePanel.Controls.Add(btnPos);
            _welcomePanel.Controls.Add(btnHelp);
            _welcomePanel.Controls.Add(hint);

            // Welcome is NOT an MDI child - sits behind; hide when POS opens
            this.Controls.Add(_welcomePanel);
            _welcomePanel.BringToFront();
            // Keep menu on top
            menuStrip1.BringToFront();
            statusStrip1.BringToFront();
        }

        private void HideWelcome()
        {
            if (_welcomePanel != null && _welcomePanel.Visible)
                _welcomePanel.Visible = false;
        }

        private void OpenPos()
        {
            HideWelcome();
            OpenChild(new PosForm());
        }

        private void OpenModule(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            key = key.ToUpperInvariant();
            if (key.Contains("POS") || key.Contains("SALE") || key.Contains("BILL"))
                OpenPos();
            else
                MessageBox.Show("Module '" + key + "' coming soon.\n\nUse: Sale → POS / Billing", "PHARMA", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            child.WindowState = FormWindowState.Maximized;
            child.Show();
        }

        private void UpdateStatus()
        {
            var u = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "-";
            statusLabel.Text = "User: " + u + "  |  Database: PharmaZ  |  Ctrl+S = POS  |  F1 = Help";
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "Keyboard Shortcuts:\n\n" +
                "Ctrl+S   Open POS / Billing\n" +
                "F2       New sale (inside POS)\n" +
                "F5       Save sale (inside POS)\n" +
                "Enter    Add item after barcode\n" +
                "Esc      Close form\n" +
                "Alt+F4   Exit application\n\n" +
                "Menu: Sale → POS / Billing",
                "PHARMA Help", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MainMdiForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                OpenPos();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F1)
            {
                ShowHelp();
                e.Handled = true;
            }
        }
    }
}
