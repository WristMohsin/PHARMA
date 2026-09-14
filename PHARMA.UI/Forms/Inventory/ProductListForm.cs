using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;
using PHARMA.UI.Helpers;

namespace PHARMA.UI.Forms.Inventory
{
    public class ProductListForm : Form
    {
        private readonly ProductService _svc = new ProductService();

        private TextBox txtSearch;
        private DataGridView dgv;
        private Label lblInfo;
        private TextBox txtCode, txtName, txtPack, txtUnit, txtTp, txtRp, txtPurRate, txtBarcode, txtStock;
        private CheckBox chkActive;
        private Button btnNew, btnSave, btnClear, btnSearch, btnLow, btnClose, btnRefresh;
        private bool _isNew;
        private bool _dirty;
        private string _loadedCode;

        public ProductListForm()
        {
            Text = "Products / New Item";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Microsoft Sans Serif", 9F);
            FormClosing += ProductListForm_FormClosing;
            BuildUI();
            ClearDetail(true);
            LoadData("");
            KeyDown += ProductListForm_KeyDown;
        }

        private void ProductListForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                if (_dirty)
                {
                    if (!UiStyle.ConfirmClose(this, "Products"))
                        e.Cancel = true;
                }
            }
        }

        private void BuildUI()
        {
            var tool = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(245, 240, 225)
            };
            txtSearch = new TextBox
            {
                Location = new Point(8, 10),
                Size = new Size(260, 24),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    LoadData(txtSearch.Text.Trim());
                    e.SuppressKeyPress = true;
                }
            };
            btnSearch = new Button { Text = "Search", Location = new Point(274, 8), Size = new Size(70, 28) };
            btnRefresh = new Button { Text = "Refresh", Location = new Point(348, 8), Size = new Size(70, 28) };
            btnLow = new Button { Text = "Low Stock", Location = new Point(422, 8), Size = new Size(80, 28) };
            btnNew = new Button { Text = "New (F2)", Location = new Point(520, 8), Size = new Size(78, 28) };
            btnSave = new Button { Text = "Save (F5)", Location = new Point(602, 8), Size = new Size(78, 28) };
            btnClear = new Button { Text = "Clear", Location = new Point(684, 8), Size = new Size(60, 28) };
            btnClose = new Button { Text = "Close (Esc)", Location = new Point(748, 8), Size = new Size(84, 28) };

            btnSearch.Click += (s, e) => LoadData(txtSearch.Text.Trim());
            btnRefresh.Click += (s, e) => LoadData(txtSearch.Text.Trim());
            btnLow.Click += (s, e) => LoadLowStock();
            btnNew.Click += (s, e) => StartNew();
            btnSave.Click += (s, e) => Save();
            btnClear.Click += (s, e) => ClearDetail(false);
            btnClose.Click += (s, e) => Close();

            tool.Controls.AddRange(new Control[]
            {
                txtSearch, btnSearch, btnRefresh, btnLow, btnNew, btnSave, btnClear, btnClose
            });

            var detail = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 150,
                BackColor = Color.FromArgb(255, 250, 235),
                Padding = new Padding(6)
            };

            int y = 8;
            detail.Controls.Add(Lbl("Code", 8, y + 2));
            txtCode = Tb(50, y, 90);
            detail.Controls.Add(txtCode);

            detail.Controls.Add(Lbl("Name", 150, y + 2));
            txtName = Tb(190, y, 280);
            detail.Controls.Add(txtName);

            detail.Controls.Add(Lbl("Pack", 480, y + 2));
            txtPack = Tb(515, y, 70);
            detail.Controls.Add(txtPack);

            detail.Controls.Add(Lbl("Unit", 595, y + 2));
            txtUnit = Tb(630, y, 50);
            detail.Controls.Add(txtUnit);

            detail.Controls.Add(Lbl("Active", 700, y + 2));
            chkActive = new CheckBox { Location = new Point(745, y), Checked = true, AutoSize = true };
            detail.Controls.Add(chkActive);

            y = 42;
            detail.Controls.Add(Lbl("TP", 8, y + 2));
            txtTp = Tb(50, y, 80);
            detail.Controls.Add(txtTp);

            detail.Controls.Add(Lbl("RP", 140, y + 2));
            txtRp = Tb(170, y, 80);
            detail.Controls.Add(txtRp);

            detail.Controls.Add(Lbl("Pur Rate", 260, y + 2));
            txtPurRate = Tb(320, y, 80);
            detail.Controls.Add(txtPurRate);

            detail.Controls.Add(Lbl("Barcode", 410, y + 2));
            txtBarcode = Tb(465, y, 140);
            detail.Controls.Add(txtBarcode);

            detail.Controls.Add(Lbl("Stock", 620, y + 2));
            txtStock = Tb(660, y, 70);
            txtStock.ReadOnly = true;
            txtStock.BackColor = Color.WhiteSmoke;
            detail.Controls.Add(txtStock);

            y = 78;
            lblInfo = new Label
            {
                Location = new Point(8, y),
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = "F2 New  F5 Save  F3 Search  Esc Close  |  Double-click row to edit"
            };
            detail.Controls.Add(lblInfo);

            EventHandler mark = (s, e) => { _dirty = true; };
            foreach (var t in new[] { txtCode, txtName, txtPack, txtUnit, txtTp, txtRp, txtPurRate, txtBarcode })
                t.TextChanged += mark;
            chkActive.CheckedChanged += mark;

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            UiStyle.StyleGrid(dgv);
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
            dgv.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter && dgv.CurrentRow != null)
                {
                    LoadFromGridRow(dgv.CurrentRow);
                    e.Handled = true;
                }
            };

            Controls.Add(dgv);
            Controls.Add(detail);
            Controls.Add(tool);
        }

        private static Label Lbl(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true };
        }

        private static TextBox Tb(int x, int y, int w)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(w, 22),
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void LoadData(string term)
        {
            try
            {
                var list = _svc.Search(term);
                BindGrid(list);
                lblInfo.Text = "Records: " + list.Count + "  |  F2 New  F5 Save  F3 Search  Esc Close";
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ProductListForm.LoadData: " + ex.Message);
                MessageBox.Show("Could not load products. Check the database connection.", "Products",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadLowStock()
        {
            try
            {
                var list = _svc.GetLowStock(10);
                BindGrid(list);
                lblInfo.Text = "Low stock (\u226410): " + list.Count;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ProductListForm.LoadLowStock: " + ex.Message);
                MessageBox.Show("Could not load low-stock products.", "Products",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BindGrid(System.Collections.Generic.List<Product> list)
        {
            dgv.DataSource = null;
            dgv.DataSource = list.Select(p => new
            {
                Code = p.pcode,
                Name = p.name1,
                Pack = p.pack,
                Unit = p.unit,
                Stock = p.balance,
                TP = p.tp,
                RP = p.rp,
                PurRate = p.Pur_Rate,
                Barcode = p.BarCode1,
                Active = p.Active
            }).ToList();
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            LoadFromGridRow(dgv.Rows[e.RowIndex]);
        }

        private void LoadFromGridRow(DataGridViewRow row)
        {
            if (row == null || row.Cells["Code"].Value == null) return;
            string code = row.Cells["Code"].Value.ToString();
            try
            {
                var p = _svc.Get(code);
                if (p == null)
                {
                    MessageBox.Show("Product not found: " + code, "Products", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                _isNew = false;
                _loadedCode = p.pcode;
                txtCode.Text = p.pcode ?? "";
                txtCode.ReadOnly = true;
                txtCode.BackColor = Color.WhiteSmoke;
                txtName.Text = p.name1 ?? "";
                txtPack.Text = p.pack ?? "";
                txtUnit.Text = p.unit > 0 ? p.unit.ToString() : "1";
                txtTp.Text = p.tp.ToString("0.####");
                txtRp.Text = p.rp.ToString("0.####");
                txtPurRate.Text = p.Pur_Rate.ToString("0.####");
                txtBarcode.Text = p.BarCode1 ?? "";
                txtStock.Text = p.balance.ToString();
                chkActive.Checked = string.IsNullOrEmpty(p.Active) || string.Equals(p.Active, "Y", StringComparison.OrdinalIgnoreCase);
                _dirty = false;
                txtName.Focus();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ProductListForm.LoadFromGridRow: " + ex.Message);
                MessageBox.Show("Could not load product details.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StartNew()
        {
            if (_dirty)
            {
                var r = MessageBox.Show("Discard unsaved changes and start a new product?", "Products",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
            }
            ClearDetail(true);
            txtCode.Focus();
        }

        private void ClearDetail(bool asNew)
        {
            _isNew = asNew;
            _loadedCode = null;
            txtCode.ReadOnly = false;
            txtCode.BackColor = Color.White;
            txtCode.Clear();
            txtName.Clear();
            txtPack.Clear();
            txtUnit.Text = "1";
            txtTp.Clear();
            txtRp.Clear();
            txtPurRate.Clear();
            txtBarcode.Clear();
            txtStock.Text = "0";
            chkActive.Checked = true;
            _dirty = false;
        }

        private void Save()
        {
            string code = txtCode.Text.Trim();
            string name = txtName.Text.Trim();

            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Product code is required.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return;
            }
            if (code.Length > 50)
            {
                MessageBox.Show("Product code is too long.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCode.Focus();
                return;
            }
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Product name is required.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            int unit = 1;
            if (!string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                if (!int.TryParse(txtUnit.Text.Trim(), out unit) || unit <= 0)
                {
                    MessageBox.Show("Unit (pack size) must be a whole number greater than zero.", "Products",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtUnit.Focus();
                    return;
                }
            }

            decimal tp = 0, rp = 0, pur = 0;
            if (!string.IsNullOrWhiteSpace(txtTp.Text) && !decimal.TryParse(txtTp.Text.Trim(), out tp))
            {
                MessageBox.Show("Trade price (TP) must be a valid number.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTp.Focus();
                return;
            }
            if (!string.IsNullOrWhiteSpace(txtRp.Text) && !decimal.TryParse(txtRp.Text.Trim(), out rp))
            {
                MessageBox.Show("Retail price (RP) must be a valid number.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRp.Focus();
                return;
            }
            if (!string.IsNullOrWhiteSpace(txtPurRate.Text) && !decimal.TryParse(txtPurRate.Text.Trim(), out pur))
            {
                MessageBox.Show("Purchase rate must be a valid number.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPurRate.Focus();
                return;
            }
            if (tp < 0 || rp < 0 || pur < 0)
            {
                MessageBox.Show("Prices cannot be negative.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_isNew || !string.Equals(code, _loadedCode, StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var existing = _svc.Get(code);
                    if (existing != null)
                    {
                        MessageBox.Show("Product code already exists: " + code, "Products",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtCode.Focus();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("ProductListForm.Save duplicate check: " + ex.Message);
                    MessageBox.Show("Could not verify product code uniqueness.", "Products",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            int stock = 0;
            int.TryParse(txtStock.Text.Trim(), out stock);

            var p = new Product
            {
                pcode = code,
                name1 = name,
                pack = string.IsNullOrWhiteSpace(txtPack.Text) ? null : txtPack.Text.Trim(),
                unit = unit,
                tp = tp,
                rp = rp,
                Pur_Rate = pur,
                BarCode1 = string.IsNullOrWhiteSpace(txtBarcode.Text) ? null : txtBarcode.Text.Trim(),
                Active = chkActive.Checked ? "Y" : "N",
                balance = stock
            };

            if (!_isNew && !string.IsNullOrEmpty(_loadedCode))
            {
                try
                {
                    var cur = _svc.Get(_loadedCode);
                    if (cur != null)
                        p.balance = cur.balance;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("ProductListForm.Save preserve stock: " + ex.Message);
                }
            }

            string error;
            if (!_svc.Save(p, out error))
            {
                MessageBox.Show(error ?? "Save failed.", "Products", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Product saved: " + code, "Products", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _dirty = false;
            _isNew = false;
            _loadedCode = code;
            txtCode.ReadOnly = true;
            txtCode.BackColor = Color.WhiteSmoke;
            LoadData(txtSearch.Text.Trim());
        }

        private void ProductListForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { StartNew(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { Save(); e.Handled = true; }
            else if (e.KeyCode == Keys.F3) { txtSearch.Focus(); txtSearch.SelectAll(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
        }
    }
}
