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
            Text = "PHARMA - Pharmacy Management";
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            Load += MainMdiForm_Load;
            KeyDown += MainMdiForm_KeyDown;
        }

        private void MainMdiForm_Load(object sender, EventArgs e)
        {
            BuildMenus();
            BuildDashboard();
            UpdateStatus();
        }

        private void BuildMenus()
        {
            menuStrip1.Items.Clear();
            try
            {
                MenuBuilder.Build(menuStrip1, _auth, OpenModule);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine("BuildMenus: " + ex.Message);
            }
        }

        private void BuildDashboard()
        {
            if (_welcomePanel != null)
            {
                Controls.Remove(_welcomePanel);
                _welcomePanel.Dispose();
            }

            _welcomePanel = new Panel();
            _welcomePanel.Dock = DockStyle.Fill;
            _welcomePanel.BackColor = Color.FromArgb(245, 248, 250);

            var title = new Label();
            title.Text = "PHARMA Dashboard";
            title.Font = new Font("Segoe UI", 22F, FontStyle.Bold);
            title.ForeColor = Color.FromArgb(0, 90, 140);
            title.AutoSize = true;
            title.Location = new Point(40, 30);

            var sub = new Label();
            string user = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "";
            sub.Text = "Welcome, " + user;
            sub.Font = new Font("Segoe UI", 12F);
            sub.AutoSize = true;
            sub.Location = new Point(40, 75);

            var stats = new Label();
            stats.Font = new Font("Segoe UI", 11F);
            stats.AutoSize = true;
            stats.Location = new Point(40, 115);
            try
            {
                decimal today = _saleSvc.GetTodayTotal();
                stats.Text = "Today's sales: " + today.ToString("N2");
            }
            catch
            {
                stats.Text = "Today's sales: (unavailable)";
            }

            var allowed = MenuBuilder.GetAuthorizedModuleKeys(_auth);
            int y = 170;
            int col = 0;
            System.Action<string, string, string> addBtn = delegate(string btnTitle, string sc, string key)
            {
                if (allowed.Count == 0) return;
                if (!allowed.Contains(key)) return;
                int x = (col % 2 == 0) ? 40 : 340;
                if (col > 0 && col % 2 == 0) y += 70;
                _welcomePanel.Controls.Add(MakeBigButton(btnTitle, sc, x, y, delegate { OpenModule(key); }));
                col++;
            };
            addBtn("POS / Billing", "Ctrl+S", "POS");
            addBtn("Purchase Entry", "Ctrl+P", "PURCHASE");
            addBtn("Products / Stock", "Ctrl+I", "PRODUCTS");
            addBtn("Parties / Accounts", "Ctrl+A", "ACCOUNTS");
            addBtn("Sale History", "", "SALE_HISTORY");
            addBtn("Sale Return", "", "SALE_RETURN");

            var hint = new Label();
            hint.Text = "Menus load from UserRights. Close child windows to return here.";
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

        private void UpdateStatus()
        {
            try
            {
                string user = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "";
                statusLabel.Text = "User: " + user;
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

            string moduleKey = AuthService.NormalizeModuleKey(key);
            if (string.IsNullOrEmpty(moduleKey))
            {
                MessageBox.Show("Unknown module.", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!_auth.HasModuleRight(moduleKey))
            {
                MessageBox.Show(
                    "You do not have permission to access this module.",
                    "Access Denied",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (moduleKey == "POS")
                OpenChild(new PosForm());
            else if (moduleKey == "SALE_RETURN")
                OpenChild(new SaleReturnForm());
            else if (moduleKey == "SALE_HISTORY")
                OpenChild(new SaleListForm());
            else if (moduleKey == "PURCHASE")
                OpenChild(new PurchaseForm());
            else if (moduleKey == "PRODUCTS" || moduleKey == "RPT_PRODUCTS")
                OpenChild(new ProductListForm());
            else if (moduleKey == "PAYMENT")
                OpenChild(new PaymentForm());
            else if (moduleKey == "ACCOUNTS" || moduleKey == "RPT_CUSTOMERS")
                OpenChild(new AccountListForm());
            else if (moduleKey == "RPT_COMPANIES" || moduleKey == "COMPANIES")
                OpenChild(new CompanyListForm());
            else
            {
                string title = ResolveModuleTitle(moduleKey);
                ComingSoonForm.ShowFor(this, moduleKey, title);
            }
        }

        private static string ResolveModuleTitle(string moduleKey)
        {
            if (string.IsNullOrEmpty(moduleKey)) return moduleKey;
            foreach (var m in ModuleCatalog.All)
            {
                if (string.Equals(m.Key, moduleKey, StringComparison.OrdinalIgnoreCase))
                    return m.OptionTitle;
            }
            return moduleKey;
        }

        private void OpenChild(Form child)
        {
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
            child.FormClosed += Child_FormClosed;
            child.Show();
            if (_welcomePanel != null)
                _welcomePanel.Visible = false;
        }

        private void Child_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (MdiChildren.Length == 0 && _welcomePanel != null)
                _welcomePanel.Visible = true;
        }

        private void ShowHelp()
        {
            MessageBox.Show(
                "Shortcuts:\nCtrl+S POS\nCtrl+P Purchase\nCtrl+I Products\nCtrl+A Accounts\nF1 Help",
                "PHARMA Help",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void MainMdiForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S) { OpenModule("POS"); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.P) { OpenModule("PURCHASE"); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.I) { OpenModule("PRODUCTS"); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.A) { OpenModule("ACCOUNTS"); e.Handled = true; }
            else if (e.KeyCode == Keys.F1) { ShowHelp(); e.Handled = true; }
        }
    }
}
