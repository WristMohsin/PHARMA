using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Services;
using PHARMA.Models;

namespace PHARMA.UI.Forms.Accounts
{
    public class AccountListForm : Form
    {
        private readonly AccountService _svc = new AccountService();
        private TextBox txtSearch;
        private DataGridView dgv;
        private Label lblInfo;

        public AccountListForm()
        {
            Text = "Accounts / Parties";
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            BuildUI();
            LoadData("");
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) Close();
                if (e.KeyCode == Keys.F3) { txtSearch.Focus(); txtSearch.SelectAll(); }
            };
        }

        private void BuildUI()
        {
            txtSearch = new TextBox { Location = new Point(20, 15), Size = new Size(300, 28), Font = new Font("Segoe UI", 11F) };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { LoadData(txtSearch.Text.Trim()); e.SuppressKeyPress = true; } };

            var btnSearch = new Button { Text = "Search", Location = new Point(330, 12), Size = new Size(90, 32) };
            btnSearch.Click += (s, e) => LoadData(txtSearch.Text.Trim());

            var btnClose = new Button { Text = "Close (Esc)", Location = new Point(430, 12), Size = new Size(100, 32) };
            btnClose.Click += (s, e) => Close();

            lblInfo = new Label { Location = new Point(20, 50), AutoSize = true };

            dgv = new DataGridView
            {
                Location = new Point(20, 75),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Size = new Size(900, 450),
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.AddRange(new Control[] { txtSearch, btnSearch, btnClose, lblInfo, dgv });
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (dgv != null)
                dgv.Size = new Size(ClientSize.Width - 40, ClientSize.Height - 100);
        }

        private void LoadData(string term)
        {
            try
            {
                var list = _svc.Search(term);
                dgv.DataSource = list.Select(a => new
                {
                    Code = a.acno,
                    Name = a.NAME ?? a.dsc,
                    a.Mobile,
                    a.Phone,
                    a.Address,
                    a.Balance,
                    Type = a.Partytype
                }).ToList();
                lblInfo.Text = "Records: " + list.Count + "  |  Outstanding: " + _svc.OutstandingTotal().ToString("N2");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
