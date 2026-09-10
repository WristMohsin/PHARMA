using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;
using PHARMA.UI.Helpers;
using PHARMA.UI.Forms.Sale;

namespace PHARMA.UI.Forms.Purchase
{
    public class PurchaseForm : Form
    {
        private readonly PurchaseService _svc = new PurchaseService();
        private readonly AccountService _accService = new AccountService();
        private readonly ProductService _prodService = new ProductService();

        private List<pur_det> _lines = new List<pur_det>();
        private Dictionary<string, string> _productNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Product _pendingProduct;
        private int _invNo;
        private bool _dirty;

        private TextBox txtInvNo, txtParty, txtSearch, txtQty, txtRate;
        private DataGridView dgvItems;
        private Label lblDate, lblPartyName, lblProduct, lblStock, lblGross, lblNet;
        private Button btnNew, btnSearch, btnSave, btnClose;

        public PurchaseForm()
        {
            Text = "Purchase Entry";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Segoe UI", 9.5F);
            FormClosing += PurchaseForm_FormClosing;
            BuildUI();
            NewDoc();
            KeyDown += PurchaseForm_KeyDown;
        }

        private void PurchaseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                if (_dirty || (_lines != null && _lines.Count > 0))
                {
                    if (!UiStyle.ConfirmClose(this, "Purchase Entry"))
                        e.Cancel = true;
                }
            }
        }

        private void BuildUI()
        {
            var tool = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(220, 215, 200) };
            btnNew = MakeToolBtn("New (F2)", 4, 90);
            btnSearch = MakeToolBtn("Search (F4)", 100, 100);
            btnSave = MakeToolBtn("Save (F5)", 210, 90);
            btnClose = MakeToolBtn("Close (Esc)", 310, 100);
            btnNew.Click += (s, e) => NewDoc();
            btnSearch.Click += (s, e) => OpenSearchAndPick(txtSearch.Text.Trim());
            btnSave.Click += (s, e) => Save();
            btnClose.Click += (s, e) => Close();
            tool.Controls.AddRange(new Control[] { btnNew, btnSearch, btnSave, btnClose });

            var header = new Panel { Dock = DockStyle.Top, Height = 130, BackColor = Color.FromArgb(245, 240, 225) };

            var lblInv = new Label { Text = "Pur.#", Location = new Point(12, 10), AutoSize = true };
            txtInvNo = new TextBox
            {
                Location = new Point(60, 7),
                Size = new Size(80, 24),
                ReadOnly = true,
                BackColor = Color.White
            };
            UiStyle.StyleTextBox(txtInvNo);

            lblDate = new Label
            {
                Location = new Point(160, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };

            var lblSup = new Label { Text = "Supplier", Location = new Point(12, 42), AutoSize = true };
            txtParty = new TextBox { Location = new Point(80, 39), Size = new Size(80, 24) };
            UiStyle.StyleTextBox(txtParty);
            txtParty.Leave += TxtParty_Leave;
            txtParty.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    TxtParty_Leave(s, e);
                    txtSearch.Focus();
                    e.SuppressKeyPress = true;
                }
            };

            lblPartyName = new Label
            {
                Location = new Point(170, 42),
                AutoSize = true,
                ForeColor = Color.DarkBlue,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };

            var lblProd = new Label { Text = "Product", Location = new Point(12, 78), AutoSize = true };
            txtSearch = new TextBox
            {
                Location = new Point(80, 75),
                Size = new Size(280, 26),
                Font = new Font("Consolas", 11F)
            };
            UiStyle.StyleTextBox(txtSearch);
            txtSearch.KeyDown += TxtSearch_KeyDown;

            var lblQty = new Label { Text = "Qty", Location = new Point(370, 78), AutoSize = true };
            txtQty = new TextBox
            {
                Location = new Point(400, 75),
                Size = new Size(60, 26),
                Text = "1",
                Font = new Font("Consolas", 11F)
            };
            UiStyle.StyleTextBox(txtQty);
            txtQty.KeyDown += TxtQty_KeyDown;

            var lblRate = new Label { Text = "Rate", Location = new Point(470, 78), AutoSize = true };
            txtRate = new TextBox
            {
                Location = new Point(510, 75),
                Size = new Size(80, 26),
                Font = new Font("Consolas", 11F)
            };
            UiStyle.StyleTextBox(txtRate);
            txtRate.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    txtQty.Focus();
                    txtQty.SelectAll();
                    e.SuppressKeyPress = true;
                }
            };

            lblProduct = new Label
            {
                Location = new Point(600, 42),
                Size = new Size(360, 20),
                ForeColor = Color.FromArgb(80, 60, 20)
            };
            lblStock = new Label
            {
                Location = new Point(600, 78),
                Size = new Size(360, 20),
                ForeColor = Color.DarkGreen
            };

            header.Controls.AddRange(new Control[]
            {
                lblInv, txtInvNo, lblDate, lblSup, txtParty, lblPartyName,
                lblProd, txtSearch, lblQty, txtQty, lblRate, txtRate,
                lblProduct, lblStock
            });

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = Color.FromArgb(235, 230, 215) };
            lblGross = new Label
            {
                Location = new Point(12, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold)
            };
            lblNet = new Label
            {
                Location = new Point(280, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 100, 60)
            };
            footer.Controls.Add(lblGross);
            footer.Controls.Add(lblNet);

            dgvItems = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White
            };
            UiStyle.StyleGrid(dgvItems);
            dgvItems.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete)
                {
                    RemoveSelectedLine();
                    e.Handled = true;
                }
            };

            Controls.Add(dgvItems);
            Controls.Add(footer);
            Controls.Add(header);
            Controls.Add(tool);
        }

        private Button MakeToolBtn(string text, int x, int w)
        {
            return new Button
            {
                Text = text,
                Location = new Point(x, 4),
                Size = new Size(w, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(235, 230, 215),
                Font = new Font("Segoe UI", 8.5F)
            };
        }

        private void TxtParty_Leave(object sender, EventArgs e)
        {
            int code = 0;
            int.TryParse(txtParty.Text.Trim(), out code);
            if (code <= 0)
            {
                lblPartyName.Text = "Cash / Default";
                lblPartyName.ForeColor = Color.DarkBlue;
                return;
            }

            try
            {
                var a = _accService.Get(code);
                if (a != null)
                {
                    string name = a.NAME != null ? a.NAME : (a.dsc != null ? a.dsc : code.ToString());
                    lblPartyName.Text = name + "  |  Bal: " + a.Balance.ToString("N2");
                    lblPartyName.ForeColor = Color.DarkBlue;
                }
                else
                {
                    lblPartyName.Text = "(supplier not found)";
                    lblPartyName.ForeColor = Color.DarkRed;
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("PurchaseForm: supplier lookup failed: " + ex.Message);
                lblPartyName.Text = "(lookup error)";
                lblPartyName.ForeColor = Color.DarkRed;
                MessageBox.Show("Could not look up supplier. Check database connection.", "Purchase",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void NewDoc()
        {
            if (_dirty || (_lines != null && _lines.Count > 0))
            {
                var r = MessageBox.Show(
                    "Clear current purchase and start a new document?",
                    "New Purchase",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
            }

            _lines = new List<pur_det>();
            _productNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _pendingProduct = null;
            _dirty = false;
            _invNo = _svc.NextInvNo();
            txtInvNo.Text = _invNo.ToString();
            lblDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtParty.Text = "0";
            lblPartyName.Text = "Cash / Default";
            lblPartyName.ForeColor = Color.DarkBlue;
            txtSearch.Clear();
            txtQty.Text = "1";
            txtRate.Clear();
            lblProduct.Text = "";
            lblStock.Text = "";
            RefreshGrid();
            txtSearch.Focus();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                string q = txtSearch.Text.Trim();
                if (string.IsNullOrEmpty(q))
                {
                    OpenSearchAndPick("");
                    return;
                }

                try
                {
                    var p = _svc.FindProduct(q);
                    if (p != null)
                        SetPending(p);
                    else
                        OpenSearchAndPick(q);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("PurchaseForm: product lookup failed: " + ex.Message);
                    MessageBox.Show("Product lookup failed. Check database connection.", "Purchase",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            else if (e.KeyCode == Keys.F4)
            {
                OpenSearchAndPick(txtSearch.Text.Trim());
                e.Handled = true;
            }
        }

        private void OpenSearchAndPick(string filter)
        {
            using (var dlg = new ProductSearchPopup(filter))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedProduct != null)
                    SetPending(dlg.SelectedProduct);
            }
        }

        private void SetPending(Product p)
        {
            if (p == null) return;
            _pendingProduct = p;
            string name = !string.IsNullOrEmpty(p.name1) ? p.name1 : (p.Desc1 ?? p.pcode);
            txtSearch.Text = name;
            lblProduct.Text = p.pcode + "  |  " + name;
            decimal rate = p.Pur_Rate > 0 ? p.Pur_Rate : p.tp;
            txtRate.Text = rate.ToString("0.####");
            txtQty.Text = "1";

            int stock = 0;
            try
            {
                stock = _prodService.GetStock(p.pcode);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("PurchaseForm: stock lookup failed: " + ex.Message);
            }
            lblStock.Text = "Stock: " + stock + "  |  Cost: " + rate.ToString("N2");
            txtQty.Focus();
            txtQty.SelectAll();
        }

        private void TxtQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CommitPendingLine();
            }
        }

        private void CommitPendingLine()
        {
            if (_pendingProduct == null)
            {
                MessageBox.Show("Select a product first (Enter or F4).", "Purchase",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtSearch.Focus();
                return;
            }

            int qty = 0;
            if (!int.TryParse(txtQty.Text.Trim(), out qty) || qty <= 0)
            {
                MessageBox.Show("Quantity must be a whole number greater than zero.", "Purchase",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQty.Focus();
                txtQty.SelectAll();
                return;
            }

            decimal rate = 0;
            if (!decimal.TryParse(txtRate.Text.Trim(), out rate) || rate < 0)
            {
                MessageBox.Show("Rate must be a valid number (0 or greater).", "Purchase",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRate.Focus();
                txtRate.SelectAll();
                return;
            }

            string pcode = _pendingProduct.pcode;
            string name = !string.IsNullOrEmpty(_pendingProduct.name1)
                ? _pendingProduct.name1
                : (_pendingProduct.Desc1 ?? pcode);

            var existing = _lines.FirstOrDefault(x => string.Equals(x.pcode, pcode, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                existing.qty += qty;
                existing.rate = rate;
            }
            else
            {
                _lines.Add(new pur_det
                {
                    pcode = pcode,
                    rate = rate,
                    qty = qty,
                    bonus = 0,
                    SortNo = _lines.Count + 1
                });
            }

            if (!_productNames.ContainsKey(pcode))
                _productNames[pcode] = name;
            else
                _productNames[pcode] = name;

            _pendingProduct = null;
            _dirty = true;
            txtSearch.Clear();
            txtQty.Text = "1";
            txtRate.Clear();
            lblProduct.Text = "";
            lblStock.Text = "";
            RefreshGrid();
            txtSearch.Focus();
        }

        private void RemoveSelectedLine()
        {
            if (dgvItems.CurrentRow == null || dgvItems.CurrentRow.Index < 0) return;
            if (dgvItems.CurrentRow.Cells["Code"] == null) return;
            object codeObj = dgvItems.CurrentRow.Cells["Code"].Value;
            if (codeObj == null) return;
            string code = codeObj.ToString();
            _lines.RemoveAll(x => string.Equals(x.pcode, code, StringComparison.OrdinalIgnoreCase));
            _productNames.Remove(code);
            _dirty = _lines.Count > 0;
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            int sr = 1;
            var rows = _lines.Select(x =>
            {
                string desc = _productNames.ContainsKey(x.pcode) ? _productNames[x.pcode] : x.pcode;
                return new
                {
                    Sr = sr++,
                    Description = desc,
                    Code = x.pcode,
                    Qty = x.qty,
                    Rate = x.rate,
                    Amount = Math.Round(x.qty * x.rate, 2)
                };
            }).ToList();

            dgvItems.DataSource = null;
            dgvItems.DataSource = rows;

            decimal gross = _lines.Sum(x => x.qty * x.rate);
            lblGross.Text = "Gross: " + gross.ToString("N2");
            lblNet.Text = "Net: " + gross.ToString("N2");
        }

        private void Save()
        {
            if (_lines.Count == 0)
            {
                MessageBox.Show("No items to save.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int party = 0;
            int.TryParse(txtParty.Text.Trim(), out party);
            if (party > 0)
            {
                try
                {
                    var a = _accService.Get(party);
                    if (a == null)
                    {
                        MessageBox.Show("Invalid supplier code. Correct it or use 0 for Cash/Default.", "Purchase",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtParty.Focus();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("PurchaseForm: supplier validation failed: " + ex.Message);
                    MessageBox.Show("Could not validate supplier. Check database connection.", "Purchase",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            var header = new purchase
            {
                invno = _invNo,
                invdt = DateTime.Now,
                code = party,
                grsamt = _lines.Sum(x => x.qty * x.rate),
                net = _lines.Sum(x => x.qty * x.rate),
                type = 1,
                Operator = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : ""
            };

            string error;
            if (!_svc.Save(header, _lines, out error))
            {
                MessageBox.Show(error ?? "Save failed.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            MessageBox.Show("Purchase saved. Invoice #: " + _invNo, "Purchase",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            _dirty = false;
            _lines.Clear();
            _productNames.Clear();
            NewDoc();
        }

        private void PurchaseForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2)
            {
                NewDoc();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F4)
            {
                OpenSearchAndPick(txtSearch.Text.Trim());
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.F5)
            {
                Save();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Close();
                e.Handled = true;
            }
        }
    }
}
