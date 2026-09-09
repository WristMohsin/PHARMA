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
        private int _invNo;
        private TextBox txtBarcode, txtParty, txtDisc, txtInvNo;
        private DataGridView dgvItems;
        private Label lblTotal, lblPartyName, lblGross, lblNet, lblDate;
        private Button btnSave, btnNew, btnClose, btnPrint, btnSearch;
        private string _lastPartyName = "Cash";
        private Panel headerPanel, footerPanel;

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
            var tool = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(220, 215, 200)
            };
            btnNew = MakeToolBtn("New (F2)", 4);
            btnSearch = MakeToolBtn("Product Search (F4)", 100);
            btnSave = MakeToolBtn("Save (F5)", 250);
            btnPrint = MakeToolBtn("Save+Print (F9)", 360);
            btnClose = MakeToolBtn("Close (Esc)", 500);
            btnNew.Click += (s, e) => NewSale();
            btnSearch.Click += (s, e) => OpenProductSearch("");
            btnSave.Click += (s, e) => SaveSale(false);
            btnPrint.Click += (s, e) => SaveSale(true);
            btnClose.Click += (s, e) => Close();
            tool.Controls.AddRange(new Control[] { btnNew, btnSearch, btnSave, btnPrint, btnClose });

            headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 90,
                BackColor = Color.FromArgb(245, 240, 225),
                Padding = new Padding(8)
            };

            var lblInv = new Label { Text = "Inv.#", Location = new Point(12, 12), AutoSize = true };
            txtInvNo = new TextBox { Location = new Point(55, 9), Size = new Size(90, 24), ReadOnly = true, BackColor = Color.White };
            lblDate = new Label { Text = DateTime.Now.ToString("dd/MM/yyyy"), Location = new Point(160, 12), AutoSize = true };

            var lblP = new Label { Text = "Party", Location = new Point(12, 48), AutoSize = true };
            txtParty = new TextBox { Location = new Point(55, 45), Size = new Size(70, 24), Text = "0" };
            txtParty.Leave += TxtParty_Leave;
            txtParty.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { txtBarcode.Focus(); e.SuppressKeyPress = true; }
            };
            lblPartyName = new Label
            {
                Location = new Point(135, 48),
                AutoSize = true,
                ForeColor = Color.DarkGreen,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Text = "Cash"
            };

            var lblB = new Label { Text = "Barcode / Code (Enter)  |  F4 = Search", Location = new Point(320, 12), AutoSize = true, ForeColor = Color.DarkBlue };
            txtBarcode = new TextBox
            {
                Location = new Point(320, 42),
                Size = new Size(320, 28),
                Font = new Font("Consolas", 13F)
            };
            txtBarcode.KeyDown += TxtBarcode_KeyDown;

            headerPanel.Controls.AddRange(new Control[] {
                lblInv, txtInvNo, lblDate, lblP, txtParty, lblPartyName, lblB, txtBarcode
            });

            footerPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 70,
                BackColor = Color.FromArgb(245, 240, 225)
            };

            var lblD = new Label { Text = "Discount", Location = new Point(12, 12), AutoSize = true };
            txtDisc = new TextBox { Location = new Point(80, 9), Size = new Size(80, 24), Text = "0" };
            txtDisc.Leave += (s, e) => RefreshGrid();

            lblGross = new Label { Text = "Gross: 0.00", Location = new Point(200, 12), AutoSize = true, Font = new Font("Segoe UI", 10F, FontStyle.Bold) };
            lblNet = new Label
            {
                Text = "INVOICE AMOUNT: 0.00",
                Location = new Point(400, 10),
                AutoSize = true,
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 100, 60)
            };
            lblTotal = lblNet;

            var hint = new Label
            {
                Text = "F2=New  F4=Product Search  F5=Save  F9=Print  Esc=Close  |  Enter on empty = Search",
                Location = new Point(12, 42),
                AutoSize = true,
                ForeColor = Color.DimGray
            };
            footerPanel.Controls.AddRange(new Control[] { lblD, txtDisc, lblGross, lblNet, hint });

            dgvItems = new DataGridView { Dock = DockStyle.Fill };
            UiStyle.StyleGrid(dgvItems);
            dgvItems.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 150, 80);
            dgvItems.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgvItems.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(255, 252, 240);

            Controls.Add(dgvItems);
            Controls.Add(footerPanel);
            Controls.Add(headerPanel);
            Controls.Add(tool);
        }

        private Button MakeToolBtn(string text, int x)
        {
            var b = new Button
            {
                Text = text,
                Location = new Point(x, 4),
                Size = new Size(text.Length > 14 ? 140 : 90, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(235, 230, 215),
                Font = new Font("Segoe UI", 8.5F)
            };
            return b;
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
                else
                {
                    lblPartyName.Text = "(not found)";
                    _lastPartyName = "";
                }
            }
            catch { lblPartyName.Text = ""; }
        }

        private void NewSale()
        {
            _lines.Clear();
            _invNo = _saleService.GetNextInvNo();
            txtInvNo.Text = _invNo.ToString();
            lblDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtParty.Text = "0";
            txtDisc.Text = "0";
            lblPartyName.Text = "Cash";
            _lastPartyName = "Cash";
            RefreshGrid();
            txtBarcode.Focus();
        }

        private void TxtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                string code = txtBarcode.Text.Trim();
                if (string.IsNullOrEmpty(code))
                    OpenProductSearch("");
                else
                {
                    var p = _saleService.FindProduct(code);
                    if (p != null)
                    {
                        AddProduct(p);
                        txtBarcode.Clear();
                    }
                    else
                        OpenProductSearch(code);
                }
            }
            else if (e.KeyCode == Keys.F4)
            {
                OpenProductSearch(txtBarcode.Text.Trim());
                e.Handled = true;
            }
        }

        private void OpenProductSearch(string filter)
        {
            using (var popup = new ProductSearchPopup(filter))
            {
                if (popup.ShowDialog(this) == DialogResult.OK && popup.SelectedProduct != null)
                {
                    AddProduct(popup.SelectedProduct);
                    txtBarcode.Clear();
                    txtBarcode.Focus();
                }
                else
                    txtBarcode.Focus();
            }
        }

        private void AddProduct(Product p)
        {
            if (p == null) return;
            string msg;
            if (!_saleService.CanSell(p.pcode, 1, out msg))
            {
                MessageBox.Show(msg, "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (!PHARMA.Common.Constants.Stock.AllowNegativeStock)
                    return;
            }
            var existing = _lines.FirstOrDefault(x => x.pcode == p.pcode);
            if (existing != null)
                existing.qty += 1;
            else
            {
                var d = new Sale_Detail();
                d.pcode = p.pcode;
                d.rate = p.rp > 0 ? p.rp : p.tp;
                d.qty = 1;
                d.bonus = 0;
                d.dip = 0;
                d.SortNo = _lines.Count + 1;
                _lines.Add(d);
            }
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            var rows = new List<object>();
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
                    Description = name,
                    Code = x.pcode,
                    Qty = x.qty,
                    Rate = x.rate,
                    Disc = x.dip,
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
            lblNet.Text = "INVOICE AMOUNT: " + (gross - disc).ToString("N2");
        }

        private void SaveSale(bool print)
        {
            if (_lines.Count == 0)
            {
                MessageBox.Show("No items.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            int party = 0;
            int.TryParse(txtParty.Text, out party);
            decimal disc = 0;
            decimal.TryParse(txtDisc.Text, out disc);
            decimal gross = 0;
            foreach (var x in _lines) gross += x.qty * x.rate;

            var header = new PHARMA.Models.Sale();
            header.invno = _invNo;
            header.invdt = DateTime.Now;
            header.code = party;
            header.grsamt = gross;
            header.disc = disc;
            header.Net = gross - disc;
            header.type = 1;
            header.Posted = "Y";
            header.Operator = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "";
            header.Computername = Environment.MachineName;
            header.PostTime = DateTime.Now.ToString("HH:mm:ss");

            string error;
            if (_saleService.SaveSale(header, _lines, out error))
            {
                if (print)
                {
                    try { new ReceiptPrinter().Print(header, new List<Sale_Detail>(_lines), _lastPartyName); }
                    catch (Exception ex) { MessageBox.Show("Saved but print failed: " + ex.Message); }
                }
                else
                    MessageBox.Show("Sale saved. Invoice # " + _invNo, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NewSale();
            }
            else
                MessageBox.Show("Save failed: " + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void PosForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { NewSale(); e.Handled = true; }
            else if (e.KeyCode == Keys.F4) { OpenProductSearch(txtBarcode.Text.Trim()); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { SaveSale(false); e.Handled = true; }
            else if (e.KeyCode == Keys.F9) { SaveSale(true); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
        }
    }
}
