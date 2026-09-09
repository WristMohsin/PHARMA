using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;
using PHARMA.UI.Helpers;

namespace PHARMA.UI.Forms.POS
{
    public partial class PosForm : Form
    {
        private readonly SaleService _saleService = new SaleService();
        private readonly AccountService _accService = new AccountService();
        private List<Sale_Detail> _lines = new List<Sale_Detail>();
        private int _invNo;
        private TextBox txtBarcode, txtParty, txtDisc;
        private DataGridView dgvItems;
        private Label lblTotal, lblInv, lblHint, lblPartyName;
        private Button btnSave, btnNew, btnClose, btnPrint;
        private string _lastPartyName = "";

        public PosForm()
        {
            InitializeComponent();
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            NewSale();
        }

        private void InitializeComponent()
        {
            txtBarcode = new TextBox();
            txtParty = new TextBox();
            txtDisc = new TextBox();
            dgvItems = new DataGridView();
            lblTotal = new Label();
            lblInv = new Label();
            lblHint = new Label();
            lblPartyName = new Label();
            btnSave = new Button();
            btnNew = new Button();
            btnClose = new Button();
            btnPrint = new Button();

            ((System.ComponentModel.ISupportInitialize)(dgvItems)).BeginInit();
            SuspendLayout();

            lblInv.AutoSize = true;
            lblInv.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            lblInv.Location = new Point(20, 12);

            var lblP = new Label();
            lblP.Text = "Party Code";
            lblP.Location = new Point(20, 45);
            lblP.AutoSize = true;
            txtParty.Location = new Point(100, 42);
            txtParty.Size = new Size(80, 25);
            txtParty.Text = "0";
            txtParty.Leave += TxtParty_Leave;

            lblPartyName.Location = new Point(190, 45);
            lblPartyName.AutoSize = true;
            lblPartyName.ForeColor = Color.DarkGreen;

            txtBarcode.Font = new Font("Consolas", 14F);
            txtBarcode.Location = new Point(20, 75);
            txtBarcode.Size = new Size(350, 30);
            txtBarcode.KeyDown += TxtBarcode_KeyDown;

            lblHint.Text = "Barcode/Code + Enter | F2=New F5=Save F9=Print Esc=Close";
            lblHint.Location = new Point(380, 80);
            lblHint.AutoSize = true;
            lblHint.ForeColor = Color.DarkBlue;

            dgvItems.AllowUserToAddRows = false;
            dgvItems.AllowUserToDeleteRows = false;
            dgvItems.Location = new Point(20, 115);
            dgvItems.Size = new Size(920, 360);
            dgvItems.ReadOnly = true;
            dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItems.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            var lblD = new Label();
            lblD.Text = "Discount";
            lblD.Location = new Point(20, 490);
            lblD.AutoSize = true;
            lblD.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtDisc.Location = new Point(90, 487);
            txtDisc.Size = new Size(80, 25);
            txtDisc.Text = "0";
            txtDisc.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            txtDisc.Leave += (s, e) => RefreshGrid();

            lblTotal.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            lblTotal.Location = new Point(700, 485);
            lblTotal.AutoSize = true;
            lblTotal.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblTotal.Text = "Total: 0.00";

            btnNew.Text = "New (F2)";
            btnNew.Location = new Point(20, 525);
            btnNew.Size = new Size(90, 35);
            btnNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnNew.Click += (s, e) => NewSale();

            btnSave.Text = "Save (F5)";
            btnSave.Location = new Point(120, 525);
            btnSave.Size = new Size(90, 35);
            btnSave.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnSave.Click += (s, e) => SaveSale(false);

            btnPrint.Text = "Save+Print (F9)";
            btnPrint.Location = new Point(220, 525);
            btnPrint.Size = new Size(120, 35);
            btnPrint.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnPrint.Click += (s, e) => SaveSale(true);

            btnClose.Text = "Close (Esc)";
            btnClose.Location = new Point(350, 525);
            btnClose.Size = new Size(100, 35);
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnClose.Click += (s, e) => Close();

            Controls.Add(lblInv);
            Controls.Add(lblP);
            Controls.Add(txtParty);
            Controls.Add(lblPartyName);
            Controls.Add(txtBarcode);
            Controls.Add(lblHint);
            Controls.Add(dgvItems);
            Controls.Add(lblD);
            Controls.Add(txtDisc);
            Controls.Add(lblTotal);
            Controls.Add(btnNew);
            Controls.Add(btnSave);
            Controls.Add(btnPrint);
            Controls.Add(btnClose);

            ClientSize = new Size(960, 580);
            Name = "PosForm";
            Text = "POS / Sale Billing";
            KeyDown += PosForm_KeyDown;

            ((System.ComponentModel.ISupportInitialize)(dgvItems)).EndInit();
            ResumeLayout(false);
            PerformLayout();
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
                    lblPartyName.Text = _lastPartyName;
                }
                else { lblPartyName.Text = "(not found)"; _lastPartyName = ""; }
            }
            catch { lblPartyName.Text = ""; }
        }

        private void NewSale()
        {
            _lines.Clear();
            _invNo = _saleService.GetNextInvNo();
            lblInv.Text = "Invoice #: " + _invNo;
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
                AddItem(txtBarcode.Text.Trim());
                txtBarcode.Clear();
            }
        }

        private void AddItem(string code)
        {
            if (string.IsNullOrEmpty(code)) return;
            var p = _saleService.FindProduct(code);
            if (p == null)
            {
                MessageBox.Show("Product not found: " + code, "POS", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            string msg;
            if (!_saleService.CanSell(p.pcode, 1, out msg))
            {
                MessageBox.Show(msg, "Stock", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var existing = _lines.FirstOrDefault(x => x.pcode == p.pcode);
            if (existing != null) existing.qty += 1;
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
            dgvItems.DataSource = null;
            dgvItems.DataSource = _lines.Select(x => new { Code = x.pcode, Qty = x.qty, Rate = x.rate, Amount = x.qty * x.rate }).ToList();
            decimal gross = 0;
            foreach (var x in _lines) gross += x.qty * x.rate;
            decimal disc = 0;
            decimal.TryParse(txtDisc.Text, out disc);
            lblTotal.Text = "Total: " + (gross - disc).ToString("N2");
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
            else if (e.KeyCode == Keys.F5) { SaveSale(false); e.Handled = true; }
            else if (e.KeyCode == Keys.F9) { SaveSale(true); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
        }
    }
}
