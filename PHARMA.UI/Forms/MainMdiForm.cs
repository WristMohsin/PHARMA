using System;
using System.Drawing;
using System.Windows.Forms;
using PHARMA.Services;
using PHARMA.UI.Helpers;
using PHARMA.UI.Forms.POS;
using PHARMA.UI.Forms.Inventory;
using PHARMA.UI.Forms.Purchase;
using PHARMA.UI.Forms.Accounts;

namespace PHARMA.UI.Forms
{
    public partial class MainMdiForm : Form
    {
        private readonly AuthService _auth = new AuthService();
        private readonly SaleService _saleSvc = new SaleService();
        private readonly ProductService _prodSvc = new ProductService();
        private readonly AccountService _accSvc = new AccountService();
        private Panel _welcomePanel;

        public MainMdiForm()
        {
            InitializeComponent();
            IsMdiContainer = true;
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            BackColor = Color.FromArgb(240, 240, 245);
            try { BuildMenus(); } catch { BuildHardcodedMenu(); }
            ShowDashboard();
            UpdateStatus();
        }

        private void BuildMenus()
        {
            menuStrip1.Items.Clear();
            BuildHardcodedMenu();
            try { MenuBuilder.Build(menuStrip1, _auth, OpenModule); } catch { }
        }

        private void BuildHardcodedMenu()
        {
            var file = new ToolStripMenuItem("&File");
            var exit = new ToolStripMenuItem("E&xit") { ShortcutKeys = Keys.Alt | Keys.F4 };
            exit.Click += (s, e) => Application.Exit();
            file.DropDownItems.Add(exit);
            menuStrip1.Items.Add(file);

            var sale = new ToolStripMenuItem("&Sale");
            var pos = new ToolStripMenuItem("&POS / Billing") { ShortcutKeys = Keys.Control | Keys.S };
            pos.Click += (s, e) => OpenPos();
            sale.DropDownItems.Add(pos);
            menuStrip1.Items.Add(sale);

            var pur = new ToolStripMenuItem("&Purchase");
            var purEntry = new ToolStripMenuItem("&Purchase Entry") { ShortcutKeys = Keys.Control | Keys.P };
            purEntry.Click += (s, e) => OpenChild(new PurchaseForm());
            pur.DropDownItems.Add(purEntry);
            menuStrip1.Items.Add(pur);

            var inv = new ToolStripMenuItem("&Inventory");
            var products = new ToolStripMenuItem("&Products") { ShortcutKeys = Keys.Control | Keys.I };
            products.Click += (s, e) => OpenChild(new ProductListForm());
            inv.DropDownItems.Add(products);
            menuStrip1.Items.Add(inv);

            var acc = new ToolStripMenuItem("&Accounts");
            var parties = new ToolStripMenuItem("&Parties / Accounts") { ShortcutKeys = Keys.Control | Keys.A };
            parties.Click += (s, e) => OpenChild(new AccountListForm());
            acc.DropDownItems.Add(parties);
            menuStrip1.Items.Add(acc);

            var help = new ToolStripMenuItem("&Help");
            var sc = new ToolStripMenuItem("&Shortcuts (F1)");
            sc.Click += (s, e) => ShowHelp();
            help.DropDownItems.Add(sc);
            menuStrip1.Items.Add(help);
        }

        private void ShowDashboard()
        {
            _welcomePanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 247, 250) };

            var title = new Label
            {
                Text = "PHARMA / PharmaZ",
                Font = new Font("Segoe UI", 26F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 60, 120),
                AutoSize = true,
                Location = new Point(40, 30)
            };

            var user = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "-";
            var sub = new Label
            {
                Text = "Welcome, " + user,
                Font = new Font("Segoe UI", 12F),
                AutoSize = true,
                Location = new Point(40, 80)
            };

            decimal todaySale = 0;
            int lowStock = 0;
            decimal outstanding = 0;
            try { todaySale = _saleSvc.GetTodayTotal(); } catch { }
            try { lowStock = _prodSvc.GetLowStock(10).Count; } catch { }
            try { outstanding = _accSvc.OutstandingTotal(); } catch { }

            var stats = new Label
            {
                Text = string.Format("Today's Sale: {0:N2}     |     Low Stock Items: {1}     |     Outstanding: {2:N2}",
                    todaySale, lowStock, outstanding),
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 100, 80),
                AutoSize = true,
                Location = new Point(40, 115)
            };

            int y = 170;
            _welcomePanel.Controls.Add(MakeBigButton("POS / Billing", "Ctrl+S", 40, y, OpenPos));
            _welcomePanel.Controls.Add(MakeBigButton("Purchase Entry", "Ctrl+P", 340, y, () => OpenChild(new PurchaseForm())));
            y += 70;
            _welcomePanel.Controls.Add(MakeBigButton("Products / Stock", "Ctrl+I", 40, y, () => OpenChild(new ProductListForm())));
            _welcomePanel.Controls.Add(MakeBigButton("Parties / Accounts", "Ctrl+A", 340, y, () => OpenChild(new AccountListForm())));

            var hint = new Label
            {
                Text = "Top menu: File | Sale | Purchase | Inventory | Accounts | Help\nF1 = Shortcuts",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.DimGray,
                AutoSize = true,
                Location = new Point(40, 340)
            };

            _welcomePanel.Controls.Add(title);
            _welcomePanel.Controls.Add(sub);
            _welcomePanel.Controls.Add(stats);
            _welcomePanel.Controls.Add(hint);
            Controls.Add(_welcomePanel);
            _welcomePanel.BringToFront();
            menuStrip1.BringToFront();
            statusStrip1.BringToFront();
        }

        private Button MakeBigButton(string text, string shortcut, int x, int y, Action onClick)
        {
            var b = new Button
            {
                Text = text + "\n(" + shortcut + ")",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Size = new Size(280, 55),
                Location = new Point(x, y),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            b.Click += (s, e) => onClick();
            return b;
        }

        private void HideDashboard()
        {
            if (_welcomePanel != null) _welcomePanel.Visible = false;
        }

        private void OpenPos()
        {
            HideDashboard();
            OpenChild(new PosForm());
        }

        private void OpenModule(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            key = key.ToUpperInvariant();
            if (key.Contains("POS") || key.Contains("SALE") || key.Contains("BILL")) OpenPos();
            else if (key.Contains("PUR")) { HideDashboard(); OpenChild(new PurchaseForm()); }
            else if (key.Contains("PROD") || key.Contains("STOCK") || key.Contains("INV")) { HideDashboard(); OpenChild(new ProductListForm()); }
            else if (key.Contains("ACC") || key.Contains("PARTY")) { HideDashboard(); OpenChild(new AccountListForm()); }
            else MessageBox.Show("Module: " + key, "PHARMA");
        }

        private void OpenChild(Form child)
        {
            HideDashboard();
            foreach (Form f in MdiChildren)
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
            statusLabel.Text = "User: " + u + "  |  DB: PharmaZ  |  Ctrl+S=POS  Ctrl+P=Purchase  Ctrl+I=Products  Ctrl+A=Accounts  F1=Help";
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "Ctrl+S  POS / Billing\nCtrl+P  Purchase Entry\nCtrl+I  Products\nCtrl+A  Accounts\nF1      This help\nF2      New (in entry screens)\nF5      Save\nEsc     Close form\nAlt+F4  Exit",
                "Shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MainMdiForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S) { OpenPos(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.P) { HideDashboard(); OpenChild(new PurchaseForm()); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.I) { HideDashboard(); OpenChild(new ProductListForm()); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.A) { HideDashboard(); OpenChild(new AccountListForm()); e.Handled = true; }
            else if (e.KeyCode == Keys.F1) { ShowHelp(); e.Handled = true; }
        }
    }
}
