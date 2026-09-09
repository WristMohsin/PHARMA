using System;
using System.Drawing;
using System.Windows.Forms;
using PHARMA.Services;
using PHARMA.UI.Helpers;
using PHARMA.UI.Forms.POS;
using PHARMA.UI.Forms.Sale;
using PHARMA.UI.Forms.Inventory;
using PHARMA.UI.Forms.Purchase;
using PHARMA.UI.Forms.Accounts;
using PHARMA.UI.Forms.Masters;

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
            BackColor = Color.FromArgb(245, 247, 250);
            try { BuildMenus(); } catch { BuildHardcodedMenu(); }
            ShowDashboard();
            UpdateStatus();
            FormClosing += MainMdiForm_FormClosing;
            MdiChildActivate += MainMdiForm_MdiChildActivate;
        }

        private void MainMdiForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                if (!UiStyle.ConfirmAppExit())
                    e.Cancel = true;
            }
        }

        private void MainMdiForm_MdiChildActivate(object sender, EventArgs e)
        {
            if (ActiveMdiChild == null)
                RestoreDashboard();
            else
                SendDashboardBack();
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
            var exit = new ToolStripMenuItem("E&xit");
            exit.ShortcutKeys = Keys.Alt | Keys.F4;
            exit.Click += (s, e) => Close();
            file.DropDownItems.Add(exit);
            menuStrip1.Items.Add(file);

            var sale = new ToolStripMenuItem("&Sale");
            var pos = new ToolStripMenuItem("&POS / Billing");
            pos.ShortcutKeys = Keys.Control | Keys.S;
            pos.Click += (s, e) => OpenChild(new PosForm());
            var hist = new ToolStripMenuItem("Sale &History");
            hist.Click += (s, e) => OpenChild(new SaleListForm());
            var ret = new ToolStripMenuItem("Sale &Return");
            ret.Click += (s, e) => OpenChild(new SaleReturnForm());
            sale.DropDownItems.Add(pos);
            sale.DropDownItems.Add(hist);
            sale.DropDownItems.Add(ret);
            menuStrip1.Items.Add(sale);

            var pur = new ToolStripMenuItem("&Purchase");
            var purEntry = new ToolStripMenuItem("&Purchase Entry");
            purEntry.ShortcutKeys = Keys.Control | Keys.P;
            purEntry.Click += (s, e) => OpenChild(new PurchaseForm());
            pur.DropDownItems.Add(purEntry);
            menuStrip1.Items.Add(pur);

            var inv = new ToolStripMenuItem("&Inventory");
            var products = new ToolStripMenuItem("&Products");
            products.ShortcutKeys = Keys.Control | Keys.I;
            products.Click += (s, e) => OpenChild(new ProductListForm());
            inv.DropDownItems.Add(products);
            menuStrip1.Items.Add(inv);

            var acc = new ToolStripMenuItem("&Accounts");
            var parties = new ToolStripMenuItem("&Parties / Accounts");
            parties.ShortcutKeys = Keys.Control | Keys.A;
            parties.Click += (s, e) => OpenChild(new AccountListForm());
            var pay = new ToolStripMenuItem("&Payment / Receipt");
            pay.Click += (s, e) => OpenChild(new PaymentForm());
            acc.DropDownItems.Add(parties);
            acc.DropDownItems.Add(pay);
            menuStrip1.Items.Add(acc);

            var mst = new ToolStripMenuItem("&Masters");
            var cmp = new ToolStripMenuItem("&Companies");
            cmp.Click += (s, e) => OpenChild(new CompanyListForm());
            mst.DropDownItems.Add(cmp);
            menuStrip1.Items.Add(mst);

            var help = new ToolStripMenuItem("&Help");
            var sc = new ToolStripMenuItem("&Shortcuts (F1)");
            sc.Click += (s, e) => ShowHelp();
            help.DropDownItems.Add(sc);
            menuStrip1.Items.Add(help);
        }

        private void ShowDashboard()
        {
            if (_welcomePanel != null)
            {
                _welcomePanel.Visible = true;
                _welcomePanel.SendToBack();
                return;
            }

            _welcomePanel = new Panel();
            _welcomePanel.Dock = DockStyle.Fill;
            _welcomePanel.BackColor = Color.FromArgb(245, 247, 250);

            var title = new Label();
            title.Text = "PHARMA / PharmaZ";
            title.Font = new Font("Segoe UI", 26F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(30, 60, 120);
            title.AutoSize = true;
            title.Location = new Point(40, 30);

            var user = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "-";
            var sub = new Label();
            sub.Text = "Welcome, " + user;
            sub.Font = new Font("Segoe UI", 12F);
            sub.AutoSize = true;
            sub.Location = new Point(40, 80);

            decimal todaySale = 0;
            int lowStock = 0;
            decimal outstanding = 0;
            try { todaySale = _saleSvc.GetTodayTotal(); } catch { }
            try { lowStock = _prodSvc.GetLowStock(10).Count; } catch { }
            try { outstanding = _accSvc.OutstandingTotal(); } catch { }

            var stats = new Label();
            stats.Text = string.Format("Today's Sale: {0:N2}     |     Low Stock: {1}     |     Outstanding: {2:N2}", todaySale, lowStock, outstanding);
            stats.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            stats.ForeColor = Color.FromArgb(0, 100, 80);
            stats.AutoSize = true;
            stats.Location = new Point(40, 115);

            int y = 170;
            _welcomePanel.Controls.Add(MakeBigButton("POS / Billing", "Ctrl+S", 40, y, delegate { OpenChild(new PosForm()); }));
            _welcomePanel.Controls.Add(MakeBigButton("Purchase Entry", "Ctrl+P", 340, y, delegate { OpenChild(new PurchaseForm()); }));
            y += 70;
            _welcomePanel.Controls.Add(MakeBigButton("Products / Stock", "Ctrl+I", 40, y, delegate { OpenChild(new ProductListForm()); }));
            _welcomePanel.Controls.Add(MakeBigButton("Parties / Accounts", "Ctrl+A", 340, y, delegate { OpenChild(new AccountListForm()); }));
            y += 70;
            _welcomePanel.Controls.Add(MakeBigButton("Sale History", "", 40, y, delegate { OpenChild(new SaleListForm()); }));
            _welcomePanel.Controls.Add(MakeBigButton("Sale Return", "", 340, y, delegate { OpenChild(new SaleReturnForm()); }));

            var hint = new Label();
            hint.Text = "Menu: File | Sale | Purchase | Inventory | Accounts | Masters | Help\nChild windows open on top — close them to return here.";
            hint.Font = new Font("Segoe UI", 10F);
            hint.ForeColor = Color.DimGray;
            hint.AutoSize = true;
            hint.Location = new Point(40, 400);

            _welcomePanel.Controls.Add(title);
            _welcomePanel.Controls.Add(sub);
            _welcomePanel.Controls.Add(stats);
            _welcomePanel.Controls.Add(hint);
            Controls.Add(_welcomePanel);
            _welcomePanel.SendToBack();
            menuStrip1.BringToFront();
            statusStrip1.BringToFront();
        }

        private void SendDashboardBack()
        {
            if (_welcomePanel != null)
            {
                _welcomePanel.Visible = true;
                _welcomePanel.SendToBack();
            }
        }

        private void RestoreDashboard()
        {
            if (_welcomePanel != null)
            {
                _welcomePanel.Visible = true;
                _welcomePanel.BringToFront();
                menuStrip1.BringToFront();
                statusStrip1.BringToFront();
                RefreshDashboardStats();
            }
        }

        private void RefreshDashboardStats()
        {
            try
            {
                foreach (Control c in _welcomePanel.Controls)
                {
                    var lbl = c as Label;
                    if (lbl != null && lbl.Text != null && lbl.Text.StartsWith("Today's Sale"))
                    {
                        decimal todaySale = 0;
                        int lowStock = 0;
                        decimal outstanding = 0;
                        try { todaySale = _saleSvc.GetTodayTotal(); } catch { }
                        try { lowStock = _prodSvc.GetLowStock(10).Count; } catch { }
                        try { outstanding = _accSvc.OutstandingTotal(); } catch { }
                        lbl.Text = string.Format("Today's Sale: {0:N2}     |     Low Stock: {1}     |     Outstanding: {2:N2}",
                            todaySale, lowStock, outstanding);
                        break;
                    }
                }
            }
            catch { }
        }

        private Button MakeBigButton(string text, string shortcut, int x, int y, Action onClick)
        {
            var b = new Button();
            b.Text = string.IsNullOrEmpty(shortcut) ? text : (text + "\n(" + shortcut + ")");
            b.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            b.Size = new Size(280, 55);
            b.Location = new Point(x, y);
            b.BackColor = Color.FromArgb(0, 120, 215);
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.Click += delegate { onClick(); };
            return b;
        }

        private void OpenModule(string key)
        {
            if (string.IsNullOrEmpty(key)) return;
            key = key.ToUpperInvariant();
            if (key.Contains("POS") || key.Contains("BILL")) OpenChild(new PosForm());
            else if (key.Contains("PUR")) OpenChild(new PurchaseForm());
            else if (key.Contains("PROD") || key.Contains("STOCK")) OpenChild(new ProductListForm());
            else if (key.Contains("ACC") || key.Contains("PARTY")) OpenChild(new AccountListForm());
            else if (key.Contains("RETURN")) OpenChild(new SaleReturnForm());
            else if (key.Contains("HIST") || key.Contains("SALE")) OpenChild(new SaleListForm());
            else MessageBox.Show("Module: " + key, "PHARMA");
        }

        private void OpenChild(Form child)
        {
            foreach (Form f in MdiChildren)
            {
                if (f.GetType() == child.GetType())
                {
                    f.Activate();
                    child.Dispose();
                    SendDashboardBack();
                    return;
                }
            }

            child.MdiParent = this;
            child.WindowState = FormWindowState.Maximized;
            child.FormClosed += Child_FormClosed;
            child.Show();
            SendDashboardBack();
            child.BringToFront();
            child.Activate();
        }

        private void Child_FormClosed(object sender, FormClosedEventArgs e)
        {
            BeginInvoke(new Action(delegate
            {
                if (MdiChildren.Length == 0)
                    RestoreDashboard();
            }));
        }

        private void UpdateStatus()
        {
            var u = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "-";
            statusLabel.Text = "User: " + u + "  |  DB: PharmaZ  |  Ctrl+S=POS  Ctrl+P=Purchase  Ctrl+I=Products  Ctrl+A=Accounts  F1=Help";
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "Ctrl+S  POS\nCtrl+P  Purchase\nCtrl+I  Products\nCtrl+A  Accounts\nF1  Help\nF2  New/Add\nF4  Product Search\nF5  Save\nF9  Print\nEsc  Close form\nAlt+F4  Exit app",
                "Shortcuts", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void MainMdiForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S) { OpenChild(new PosForm()); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.P) { OpenChild(new PurchaseForm()); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.I) { OpenChild(new ProductListForm()); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.A) { OpenChild(new AccountListForm()); e.Handled = true; }
            else if (e.KeyCode == Keys.F1) { ShowHelp(); e.Handled = true; }
        }
    }
}
