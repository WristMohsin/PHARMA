using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Services;
using PHARMA.Models;

namespace PHARMA.UI.Forms.Inventory
{
    public class ProductListForm : Form
    {
        private readonly ProductService _svc = new ProductService();
        private TextBox txtSearch;
        private DataGridView dgv;
        private Label lblInfo;

        public ProductListForm()
        {
            Text = "Products / Inventory";
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
            txtSearch = new TextBox { Location = new Point(20, 15), Size = new Size(350, 28), Font = new Font("Segoe UI", 11F) };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { LoadData(txtSearch.Text.Trim()); e.Handled = true; } };

            var btnSearch = new Button { Text = "Search (Enter)", Location = new Point(380, 12), Size = new Size(120, 32) };
            btnSearch.Click += (s, e) => LoadData(txtSearch.Text.Trim());

            var btnLow = new Button { Text = "Low Stock", Location = new Point(510, 12), Size = new Size(100, 32) };
            btnLow.Click += (s, e) =>
            {
                var list = _svc.GetLowStock(10);
                dgv.DataSource = list.Select(p => new { p.pcode, Name = p.name1, p.pack, Stock = p.balance, TP = p.tp, RP = p.rp, p.BarCode1 }).ToList();
                lblInfo.Text = "Low stock items: " + list.Count;
            };

            var btnClose = new Button { Text = "Close (Esc)", Location = new Point(620, 12), Size = new Size(100, 32) };
            btnClose.Click += (s, e) => Close();

            lblInfo = new Label { Location = new Point(20, 50), AutoSize = true, Text = "" };

            dgv = new DataGridView
            {
                Location = new Point(20, 75),
                Size = new Size(ClientSize.Width - 40, ClientSize.Height - 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.AddRange(new Control[] { txtSearch, btnSearch, btnLow, btnClose, lblInfo, dgv });
        }

        private void LoadData(string term)
        {
            try
            {
                var list = _svc.Search(term);
                dgv.DataSource = list.Select(p => new
                {
                    Code = p.pcode,
                    Name = p.name1,
                    Pack = p.pack,
                    Stock = p.balance,
                    TP = p.tp,
                    RP = p.rp,
                    Barcode = p.BarCode1,
                    Active = p.Active
                }).ToList();
                lblInfo.Text = "Records: " + list.Count + "  |  F3=Search  Esc=Close";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Products");
            }
        }
    }
}
