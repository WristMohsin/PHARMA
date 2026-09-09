using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;
using PHARMA.UI.Helpers;
using PHARMA.UI.Forms.Sale;

namespace PHARMA.UI.Forms.POS
{
    public partial class PosForm : Form
    {
        private readonly SaleService _saleService = new SaleService();
        private readonly AccountService _accService = new AccountService();
        private List<Sale_Detail> _lines = new List<Sale_Detail>();
        private Product _pendingProduct;
        private int _invNo;
        private TextBox txtSearch, txtQty, txtParty, txtDisc, txtInvNo;
        private DataGridView dgvItems;
        private Label lblPartyName, lblGross, lblNet, lblDate, lblProduct, lblStock;
        private Button btnSave, btnNew, btnClose, btnPrint, btnSearch;
        private string _lastPartyName = "Cash";

        public PosForm()
        {
            Text = "Sale Invoice";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Segoe UI", 9.5F);
            FormClosing += (s, e) =>
            {
                if (!UiStyle.ConfirmClose(this, "POS / Sale"))
                    e.Cancel = true;
            };
            BuildUI();
            NewSale();
            KeyDown += PosForm_KeyDown;
        }

        private void BuildUI()
        {
            var tool = new Panel { Dock = DockStyle.Top, Height = 36, BackColor = Color.FromArgb(220, 215, 200) };
            btnNew = MakeToolBtn("New (F2)", 4, 90);
            btnSearch = MakeToolBtn("Search (F4)", 100, 100);
            btnSave = MakeToolBtn("Save (F5)", 210, 90);
            btnPrint = MakeToolBtn("Print (F9)", 310, 90);
            btnClose = MakeToolBtn("Close (Esc)", 410, 100);
            btnNew.Click += (s, e) => NewSale();
            btnSearch.Click += (s, e) => OpenSearchAndPick(txtSearch.Text.Trim());
            btnSave.Click += (s, e) => BeginSave(false);
            btnPrint.Click += (s, e) => BeginSave(true);
            btnClose.Click += (s, e) => Close();
            tool.Controls.AddRange(new Control[] { btnNew, btnSearch, btnSave, btnPrint, btnClose });

            var header = new Panel { Dock = DockStyle.Top, Height = 120, BackColor = Color.FromArgb(245, 240, 225) };

            var lblInv = new Label { Text = "Inv.#", Location = new Point(12, 10), AutoSize = true };
            txtInvNo = new TextBox { Location = new Point(55, 7), Size = new Size(80, 24), ReadOnly = true, BackColor = Color.White };
            lblDate = new Label { Text = DateTime.Now.ToString("dd/MM/yyyy"), Location = new Point(150, 10), AutoSize = true };

            var lblP = new Label { Text = "Party", Location = new Point(12, 42), AutoSize = true };
            txtParty = new TextBox { Location = new Point(55, 39), Size = new Size(70, 24), Text = "0" };
            txtParty.Leave += TxtParty_Leave;
            txtParty.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { txtSearch.Focus(); e.SuppressKeyPress = true; }
            };
            lblPartyName = new Label
            {
                Location = new Point(135, 42),
                AutoSize = true,
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "Cash"
            };

            var lblDesc = new Label { Text = "Product (name / code)", Location = new Point(12, 78), AutoSize = true, ForeColor = Color.DarkBlue };
            txtSearch = new TextBox
            {
                Location = new Point(150, 74),
                Size = new Size(320, 28),
                Font = new Font("Segoe UI", 11F)
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;

            var lblQ = new Label { Text = "Qty", Location = new Point(490, 78), AutoSize = true };
            txtQty = new TextBox
            {
                Location = new Point(525, 74),
                Size = new Size(60, 28),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Text = "1"
            };
            txtQty.KeyDown += TxtQty_KeyDown;

            lblProduct = new Label
            {
                Location = new Point(600, 42),
                Size = new Size(280, 22),
                ForeColor = Color.FromArgb(0, 90, 160),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = ""
            };
            lblStock = new Label
            {
                Location = new Point(600, 78),
                AutoSize = true,
                ForeColor = Color.DimGray,
                Text = ""
            };

            header.Controls.AddRange(new Control[] {
                lblInv, txtInvNo, lblDate, lblP, txtParty, lblPartyName,
                lblDesc, txtSearch, lblQ, txtQty, lblProduct, lblStock
            });

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 58, BackColor = Color.FromArgb(245, 240, 225) };
            var lblD = new Label { Text = "Bill Disc", Location = new Point(12, 10), AutoSize = true };
            txtDisc = new TextBox { Location = new Point(75, 7), Size = new Size(70, 24), Text = "0" };
            txtDisc.Leave += (s, e) => RefreshGrid();
            lblGross = new Label { Text = "Gross: 0.00", Location = new Point(170, 10), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            lblNet = new Label
            {
                Text = "INVOICE: 0.00",
                Location = new Point(320, 8),
                AutoSize = true,
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 100, 60)
            };
            var hint = new Label
            {
                Text = "Type name + Enter = Search  |  Qty + Enter = Add line  |  F5 = Payment & Save",
                Location = new Point(12, 34),
                AutoSize = true,
                ForeColor = Color.DimGray
            };
            footer.Controls.AddRange(new Control[] { lblD, txtDisc, lblGross, lblNet, hint });

            dgvItems = new DataGridView { Dock = DockStyle.Fill };
            UiStyle.StyleGrid(dgvItems);
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 150, 80);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 252, 240);
            dgvItems.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Delete) RemoveSelectedLine();
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
            int.TryParse(txtParty.Text, out code);
            if (code <= 0) { lblPartyName.Text = "Cash"; _lastPartyName = "Cash"; return; }
            try
            {
                var a = _accService.Get(code);
                if (a != null)
                {
                    _lastPartyName = a.NAME != null ? a.NAME : (a.dsc != null ? a.dsc : code.ToString());
                    lblPartyName.Text = _lastPartyName + "  |  Bal: " + a.Balance.ToString("N2");
                }
                else { lblPartyName.Text = "(not found)"; _lastPartyName = ""; }
            }
            catch { lblPartyName.Text = ""; }
        }

        private void NewSale()
        {
            _lines.Clear();
            _pendingProduct = null;
            _invNo = _saleService.GetNextInvNo();
            txtInvNo.Text = _invNo.ToString();
            lblDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtParty.Text = "0";
            txtDisc.Text = "0";
            txtSearch.Clear();
            txtQty.Text = "1";
            lblPartyName.Text = "Cash";
            _lastPartyName = "Cash";
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
                var p = _saleService.FindProduct(q);
                if (p != null)
                    SetPending(p);
                else
                    OpenSearchAndPick(q);
            }
            else if (e.KeyCode == Keys.F4)
            {
                OpenSearchAndPick(txtSearch.Text.Trim());
                e.Handled = true;
            }
        }

        private void OpenSearchAndPick(string filter)
        {
            using (var popup = new ProductSearchPopup(filter))
            {
                if (popup.ShowDialog(this) == DialogResult.OK && popup.SelectedProduct != null)
                    SetPending(popup.SelectedProduct);
                else
                    txtSearch.Focus();
            }
        }

        private void SetPending(Product p)
        {
            _pendingProduct = p;
            txtSearch.Text = p.name1 != null ? p.name1 : p.pcode;
            lblProduct.Text = p.pcode + "  |  RP: " + (p.rp > 0 ? p.rp : p.tp).ToString("N2");
            lblStock.Text = "Stock: " + p.balance;
            txtQty.Text = "1";
            txtQty.Focus();
            txtQty.SelectAll();
        }

        private void TxtQty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                CommitLine();
            }
        }

        private void CommitLine()
        {
            if (_pendingProduct == null)
            {
                var p = _saleService.FindProduct(txtSearch.Text.Trim());
                if (p == null)
                {
                    OpenSearchAndPick(txtSearch.Text.Trim());
                    return;
                }
                _pendingProduct = p;
            }

            int qty = 1;
            int.TryParse(txtQty.Text, out qty);
            if (qty <= 0) qty = 1;

            string msg;
            if (!_saleService.CanSell(_pendingProduct.pcode, qty, out msg))
            {
                MessageBox.Show(msg, "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (!PHARMA.Common.Constants.Stock.AllowNegativeStock)
                {
                    txtQty.Focus();
                    return;
                }
            }

            var existing = _lines.FirstOrDefault(x => x.pcode == _pendingProduct.pcode);
            if (existing != null)
                existing.qty += qty;
            else
            {
                var d = new Sale_Detail();
                d.pcode = _pendingProduct.pcode;
                d.rate = _pendingProduct.rp > 0 ? _pendingProduct.rp : _pendingProduct.tp;
                d.qty = qty;
                d.bonus = 0;
                d.dip = 0;
                d.SortNo = _lines.Count + 1;
                _lines.Add(d);
            }

            RefreshGrid();
            _pendingProduct = null;
            txtSearch.Clear();
            txtQty.Text = "1";
            lblProduct.Text = "";
            lblStock.Text = "";
            txtSearch.Focus();
        }

        private void RemoveSelectedLine()
        {
            if (dgvItems.CurrentRow == null) return;
            try
            {
                var code = Convert.ToString(dgvItems.CurrentRow.Cells["Code"].Value);
                _lines.RemoveAll(x => x.pcode == code);
                RefreshGrid();
            }
            catch { }
        }

        private void RefreshGrid()
        {
            var rows = new List<object>();
            int sr = 1;
            foreach (var x in _lines)
            {
                string name = x.pcode;
                try
                {
                    var p = _saleService.FindProduct(x.pcode);
                    if (p != null && !string.IsNullOrEmpty(p.name1)) name = p.name1;
                }
                catch { }
                rows.Add(new
                {
                    Sr = sr++,
                    Description = name,
                    Code = x.pcode,
                    Qty = x.qty,
                    Rate = x.rate,
                    NetAmount = x.qty * x.rate
                });
            }
            dgvItems.DataSource = null;
            dgvItems.DataSource = rows;

            decimal gross = 0;
            foreach (var x in _lines) gross += x.qty * x.rate;
            decimal disc = 0;
            decimal.TryParse(txtDisc.Text, out disc);
            lblGross.Text = "Gross: " + gross.ToString("N2");
            lblNet.Text = "INVOICE: " + (gross - disc).ToString("N2");
        }

        private void BeginSave(bool printAfter)
        {
            if (_lines.Count == 0)
            {
                MessageBox.Show("No items in bill.", "Sale", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            decimal gross = 0;
            foreach (var x in _lines) gross += x.qty * x.rate;
            decimal billDisc = 0;
            decimal.TryParse(txtDisc.Text, out billDisc);
            decimal billAmount = gross - billDisc;
            if (billAmount < 0) billAmount = 0;

            using (var pay = new SalePaymentDialog(billAmount))
            {
                if (pay.ShowDialog(this) != DialogResult.OK)
                    return;

                decimal totalDisc = billDisc + pay.ExtraDiscount;
                int party = 0;
                int.TryParse(txtParty.Text, out party);

                var header = new PHARMA.Models.Sale();
                header.invno = _invNo;
                header.invdt = DateTime.Now;
                header.code = party;
                header.grsamt = gross;
                header.disc = totalDisc;
                header.Net = gross - totalDisc;
                if (header.Net < 0) header.Net = 0;
                header.type = 1;
                header.Posted = "Y";
                header.Operator = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "";
                header.Computername = Environment.MachineName;
                header.PostTime = DateTime.Now.ToString("HH:mm:ss");
                header.Remarks = pay.Remarks;
                header.Amt_Received = pay.CashReceived;
                header.cash_return = (int)Math.Round(pay.CashReturn);

                string error;
                if (_saleService.SaveSale(header, _lines, out error))
                {
                    if (printAfter)
                    {
                        try { new ReceiptPrinter().Print(header, new List<Sale_Detail>(_lines), _lastPartyName); }
                        catch (Exception ex) { MessageBox.Show("Saved. Print error: " + ex.Message); }
                    }
                    else
                    {
                        MessageBox.Show(
                            "Sale saved.\nInvoice # " + _invNo +
                            "\nNet: " + header.Net.ToString("N2") +
                            "\nCash: " + pay.CashReceived.ToString("N2") +
                            "\nReturn: " + pay.CashReturn.ToString("N2"),
                            "Success",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    NewSale();
                }
                else
                    MessageBox.Show("Save failed: " + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PosForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { NewSale(); e.Handled = true; }
            else if (e.KeyCode == Keys.F4) { OpenSearchAndPick(txtSearch.Text.Trim()); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { BeginSave(false); e.Handled = true; }
            else if (e.KeyCode == Keys.F9) { BeginSave(true); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
        }
    }
}
