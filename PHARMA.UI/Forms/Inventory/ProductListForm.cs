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
        private Label lblStatus;
        private Button btnNew, btnSearch, btnRefresh, btnLow, btnDeactivate, btnClose;

        public ProductListForm()
        {
            Text = "Products / New Item";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Microsoft Sans Serif", 9F);
            BuildUI();
            ReloadFromDb("");
            KeyDown += ProductListForm_KeyDown;
        }

        private void BuildUI()
        {
            var tool = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(245, 240, 225)
            };
            txtSearch = new TextBox
            {
                Location = new Point(6, 8),
                Size = new Size(220, 22),
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;
            btnSearch = MkBtn("Search", 232, 6, 64);
            btnRefresh = MkBtn("Refresh", 300, 6, 64);
            btnLow = MkBtn("Low Stock", 368, 6, 72);
            btnNew = MkBtn("New (F2)", 448, 6, 72);
            btnDeactivate = MkBtn("Deactivate", 524, 6, 80);
            btnClose = MkBtn("Close (Esc)", 608, 6, 80);
            btnSearch.Click += (s, e) => ReloadFromDb(txtSearch.Text.Trim());
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); ReloadFromDb(""); };
            btnLow.Click += (s, e) => LoadLowStock();
            btnNew.Click += (s, e) => OpenNew();
            btnDeactivate.Click += (s, e) => DeactivateSelected();
            btnClose.Click += (s, e) => Close();
            tool.Controls.AddRange(new Control[]
            {
                txtSearch, btnSearch, btnRefresh, btnLow, btnNew, btnDeactivate, btnClose
            });

            var footer = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                BackColor = Color.FromArgb(245, 240, 225)
            };
            lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(6, 0, 0, 0),
                ForeColor = Color.DimGray,
                Text = "F2 New  Enter/Double-click Edit  F3 Search  Esc Close  |  Grid is read-only"
            };
            footer.Controls.Add(lblStatus);

            dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                ReadOnly = true,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AutoGenerateColumns = false,
                EditMode = DataGridViewEditMode.EditProgrammatically
            };
            dgv.RowTemplate.Height = 22;
            dgv.ColumnHeadersHeight = 24;
            UiStyle.StyleGrid(dgv);
            BuildColumns();
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OpenEditSelected(); };
            dgv.KeyDown += Dgv_KeyDown;

            Controls.Add(dgv);
            Controls.Add(footer);
            Controls.Add(tool);
        }

        private static Button MkBtn(string text, int x, int y, int w)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, 26),
                FlatStyle = FlatStyle.System
            };
        }

        private void BuildColumns()
        {
            dgv.Columns.Clear();
            dgv.Columns.Add(Col("Code", "Code", 80));
            dgv.Columns.Add(Col("Name", "Product Name", 200));
            dgv.Columns.Add(Col("Pack", "Pack", 60));
            dgv.Columns.Add(Col("Unit", "Unit", 45));
            var stock = Col("Stock", "Stock", 60);
            stock.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dgv.Columns.Add(stock);
            var tp = Col("TP", "TP", 70);
            tp.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            tp.DefaultCellStyle.Format = "N2";
            dgv.Columns.Add(tp);
            var rp = Col("RP", "RP", 70);
            rp.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            rp.DefaultCellStyle.Format = "N2";
            dgv.Columns.Add(rp);
            var pur = Col("PurRate", "Pur Rate", 70);
            pur.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            pur.DefaultCellStyle.Format = "N2";
            dgv.Columns.Add(pur);
            dgv.Columns.Add(Col("Barcode", "Barcode", 100));
            dgv.Columns.Add(Col("Active", "Active", 50));
        }

        private static DataGridViewTextBoxColumn Col(string name, string header, int width)
        {
            return new DataGridViewTextBoxColumn
            {
                Name = name,
                DataPropertyName = name,
                HeaderText = header,
                Width = width,
                ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private void ReloadFromDb(string term)
        {
            try
            {
                var list = _svc.Search(term);
                Bind(list);
                lblStatus.Text = "Rows: " + list.Count +
                                 "  |  F2 New  Enter/Double-click Edit  F3 Search  Esc Close  |  Stock is transaction-controlled";
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ProductListForm.ReloadFromDb: " + ex.Message);
                MessageBox.Show("Could not load products. Check the database connection.",
                    "Products", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadLowStock()
        {
            try
            {
                var list = _svc.GetLowStock(10);
                Bind(list);
                lblStatus.Text = "Low stock (\u226410): " + list.Count;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ProductListForm.LoadLowStock: " + ex.Message);
                MessageBox.Show("Could not load low-stock products.", "Products",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Bind(System.Collections.Generic.List<Product> list)
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
                Active = string.IsNullOrEmpty(p.Active) || string.Equals(p.Active, "Y", StringComparison.OrdinalIgnoreCase) ? "Yes" : "No"
            }).ToList();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ReloadFromDb(txtSearch.Text.Trim());
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                dgv.Focus();
                e.Handled = true;
            }
        }

        private void OpenNew()
        {
            using (var f = ProductEditorForm.ForNew())
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    ReloadFromDb(txtSearch.Text.Trim());
                    SelectCode(f.SavedCode);
                }
            }
        }

        private void OpenEditSelected()
        {
            string code = GetSelectedCode();
            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Select a product row first.", "Products",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Product p;
            try { p = _svc.Get(code); }
            catch (Exception ex)
            {
                Trace.WriteLine("ProductListForm.OpenEdit: " + ex.Message);
                MessageBox.Show("Could not load product.", "Products",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (p == null)
            {
                MessageBox.Show("Product not found: " + code, "Products",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var f = ProductEditorForm.ForEdit(p))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    ReloadFromDb(txtSearch.Text.Trim());
                    SelectCode(f.SavedCode);
                }
            }
        }

        private string GetSelectedCode()
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0) return null;
            object v = dgv.CurrentRow.Cells["Code"].Value;
            return v != null ? v.ToString() : null;
        }

        private void SelectCode(string code)
        {
            if (string.IsNullOrEmpty(code) || dgv.Rows.Count == 0) return;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells["Code"].Value != null &&
                    string.Equals(row.Cells["Code"].Value.ToString(), code, StringComparison.OrdinalIgnoreCase))
                {
                    row.Selected = true;
                    dgv.CurrentCell = row.Cells["Code"];
                    try { dgv.FirstDisplayedScrollingRowIndex = row.Index; }
                    catch { }
                    break;
                }
            }
        }

        private void Dgv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenEditSelected();
            }
        }

        private void DeactivateSelected()
        {
            string code = GetSelectedCode();
            if (string.IsNullOrEmpty(code))
            {
                MessageBox.Show("Select a product row first.", "Products",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var r = MessageBox.Show(
                "Deactivate product " + code + " (Active = N)?\nReferenced history is kept.",
                "Products", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;
            string error;
            if (!_svc.Deactivate(code, out error))
            {
                MessageBox.Show(error ?? "Deactivate failed.", "Products",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ReloadFromDb(txtSearch.Text.Trim());
            SelectCode(code);
            lblStatus.Text = "Deactivated " + code;
        }

        private void ProductListForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { OpenNew(); e.Handled = true; }
            else if (e.KeyCode == Keys.F3)
            {
                txtSearch.Focus();
                txtSearch.SelectAll();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                if (txtSearch.Focused) { dgv.Focus(); e.Handled = true; }
                else { Close(); e.Handled = true; }
            }
        }
    }
}
