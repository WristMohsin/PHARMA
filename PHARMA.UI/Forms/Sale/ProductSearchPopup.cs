using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;
using PHARMA.UI.Helpers;

namespace PHARMA.UI.Forms.Sale
{
    public class ProductSearchPopup : Form
    {
        private readonly ProductService _svc = new ProductService();
        private List<Product> _all = new List<Product>();
        private TextBox txtSearch;
        private DataGridView dgv;
        private CheckBox chkBalance;
        private Label lblCount;

        public Product SelectedProduct { get; private set; }

        public ProductSearchPopup(string initialFilter)
        {
            Text = "Product Search";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(900, 480);
            KeyPreview = true;
            BackColor = Color.FromArgb(250, 248, 240);
            BuildUI(initialFilter);
            LoadProducts();
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
                if (e.KeyCode == Keys.Enter && !txtSearch.Focused) SelectCurrent();
            };
        }

        private void BuildUI(string initial)
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(245, 240, 225) };

            var lbl = new Label { Text = "Any Where", Location = new Point(12, 12), AutoSize = true };
            txtSearch = new TextBox
            {
                Location = new Point(90, 8),
                Size = new Size(280, 26),
                Font = new Font("Segoe UI", 11F),
                Text = initial ?? ""
            };
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Down) { dgv.Focus(); e.Handled = true; }
                if (e.KeyCode == Keys.Enter) { SelectCurrent(); e.SuppressKeyPress = true; }
            };

            chkBalance = new CheckBox
            {
                Text = "Display Balance Items",
                Location = new Point(400, 10),
                AutoSize = true
            };
            chkBalance.CheckedChanged += (s, e) => ApplyFilter();

            lblCount = new Label { Location = new Point(600, 12), AutoSize = true, ForeColor = Color.DarkBlue };

            top.Controls.AddRange(new Control[] { lbl, txtSearch, chkBalance, lblCount });

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White
            };
            UiStyle.StyleGrid(dgv);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 150, 80);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 252, 240);
            dgv.DoubleClick += (s, e) => SelectCurrent();
            dgv.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { SelectCurrent(); e.Handled = true; e.SuppressKeyPress = true; }
            };

            var bottom = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.FromArgb(245, 240, 225) };
            var btnOk = new Button { Text = "Select", Location = new Point(580, 8), Size = new Size(90, 32) };
            UiStyle.StylePrimaryButton(btnOk);
            btnOk.Click += (s, e) => SelectCurrent();
            var btnClose = new Button { Text = "Close", Location = new Point(680, 8), Size = new Size(90, 32) };
            UiStyle.StyleSecondaryButton(btnClose);
            btnClose.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            bottom.Controls.AddRange(new Control[] { btnOk, btnClose });

            Controls.Add(dgv);
            Controls.Add(bottom);
            Controls.Add(top);

            Shown += (s, e) =>
            {
                txtSearch.Focus();
                txtSearch.SelectionStart = txtSearch.Text.Length;
            };
        }

        private void LoadProducts()
        {
            try
            {
                _all = _svc.Search("");
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading products: " + ex.Message);
            }
        }

        private void ApplyFilter()
        {
            string q = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            IEnumerable<Product> list = _all;
            if (!string.IsNullOrEmpty(q))
            {
                list = _all.Where(p =>
                    (p.pcode != null && p.pcode.ToLowerInvariant().Contains(q)) ||
                    (p.name1 != null && p.name1.ToLowerInvariant().Contains(q)) ||
                    (p.BarCode1 != null && p.BarCode1.ToLowerInvariant().Contains(q)) ||
                    (p.pack != null && p.pack.ToLowerInvariant().Contains(q)) ||
                    (p.CmpCd != null && p.CmpCd.ToLowerInvariant().Contains(q)));
            }
            if (chkBalance.Checked)
                list = list.Where(p => p.balance > 0);

            var rows = list.Take(500).Select(p => new
            {
                Code = p.pcode,
                ProductName = p.name1,
                Pack = p.pack,
                Company = p.CmpCd,
                RP = p.rp,
                TP = p.tp,
                Stock = p.balance,
                Barcode = p.BarCode1,
                Active = p.Active
            }).ToList();

            dgv.DataSource = rows;
            lblCount.Text = rows.Count + " item(s)";
            if (dgv.Rows.Count > 0)
                dgv.Rows[0].Selected = true;
        }

        private void SelectCurrent()
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0)
            {
                if (dgv.Rows.Count > 0) dgv.Rows[0].Selected = true;
                else return;
            }
            var code = Convert.ToString(dgv.CurrentRow.Cells["Code"].Value);
            if (string.IsNullOrEmpty(code)) return;
            SelectedProduct = _svc.Get(code);
            if (SelectedProduct == null)
                SelectedProduct = _all.FirstOrDefault(p => p.pcode == code);
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
