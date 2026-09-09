using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;

namespace PHARMA.UI.Forms.POS
{
    /// <summary>
    /// Fast keyboard-centric POS screen.
    /// Hotkeys: F2 New, F5 Save, F3 Search, Esc Close, Enter on barcode adds item.
    /// </summary>
    public partial class PosForm : Form
    {
        private readonly SaleService _saleService = new SaleService();
        private readonly ProductService _prodService = new ProductService();
        private List<Sale_Detail> _lines = new List<Sale_Detail>();
        private int _invNo;

        public PosForm()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.WindowState = FormWindowState.Maximized;
            NewSale();
        }

        private void InitializeComponent()
        {
            this.txtBarcode = new TextBox();
            this.dgvItems = new DataGridView();
            this.lblTotal = new Label();
            this.lblInv = new Label();
            this.btnSave = new Button();
            this.btnNew = new Button();
            this.btnClose = new Button();
            this.lblHint = new Label();

            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).BeginInit();
            this.SuspendLayout();

            this.txtBarcode.Font = new Font("Consolas", 14F);
            this.txtBarcode.Location = new Point(20, 50);
            this.txtBarcode.Name = "txtBarcode";
            this.txtBarcode.Size = new Size(400, 30);
            this.txtBarcode.TabIndex = 0;
            this.txtBarcode.KeyDown += TxtBarcode_KeyDown;

            this.dgvItems.AllowUserToAddRows = false;
            this.dgvItems.AllowUserToDeleteRows = false;
            this.dgvItems.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvItems.Location = new Point(20, 100);
            this.dgvItems.Name = "dgvItems";
            this.dgvItems.ReadOnly = true;
            this.dgvItems.Size = new Size(900, 400);
            this.dgvItems.TabIndex = 1;
            this.dgvItems.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            this.lblInv.AutoSize = true;
            this.lblInv.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.lblInv.Location = new Point(20, 15);
            this.lblInv.Text = "Invoice #:";

            this.lblTotal.AutoSize = true;
            this.lblTotal.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTotal.Location = new Point(700, 520);
            this.lblTotal.Text = "Total: 0.00";

            this.btnNew.Location = new Point(20, 520);
            this.btnNew.Size = new Size(100, 35);
            this.btnNew.Text = "New (F2)";
            this.btnNew.Click += (s, e) => NewSale();

            this.btnSave.Location = new Point(130, 520);
            this.btnSave.Size = new Size(100, 35);
            this.btnSave.Text = "Save (F5)";
            this.btnSave.Click += (s, e) => SaveSale();

            this.btnClose.Location = new Point(240, 520);
            this.btnClose.Size = new Size(100, 35);
            this.btnClose.Text = "Close (Esc)";
            this.btnClose.Click += (s, e) => this.Close();

            this.lblHint.AutoSize = true;
            this.lblHint.ForeColor = Color.DarkBlue;
            this.lblHint.Location = new Point(450, 55);
            this.lblHint.Text = "Scan / Type code + Enter  |  F2=New  F5=Save  Esc=Close";

            this.Controls.Add(this.txtBarcode);
            this.Controls.Add(this.dgvItems);
            this.Controls.Add(this.lblInv);
            this.Controls.Add(this.lblTotal);
            this.Controls.Add(this.btnNew);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblHint);

            this.ClientSize = new Size(950, 580);
            this.Name = "PosForm";
            this.Text = "POS / Sale Billing";
            this.KeyDown += PosForm_KeyDown;

            ((System.ComponentModel.ISupportInitialize)(this.dgvItems)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        private TextBox txtBarcode;
        private DataGridView dgvItems;
        private Label lblTotal;
        private Label lblInv;
        private Button btnSave;
        private Button btnNew;
        private Button btnClose;
        private Label lblHint;

        private void NewSale()
        {
            _lines.Clear();
            _invNo = _saleService.GetNextInvNo();
            lblInv.Text = "Invoice #: " + _invNo;
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
            if (existing != null)
            {
                existing.qty += 1;
            }
            else
            {
                _lines.Add(new Sale_Detail
                {
                    pcode = p.pcode,
                    rate = p.rp > 0 ? p.rp : p.tp,
                    qty = 1,
                    bonus = 0,
                    dip = 0,
                    SortNo = _lines.Count + 1
                });
            }
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            dgvItems.DataSource = null;
            dgvItems.DataSource = _lines.Select(x => new
            {
                Code = x.pcode,
                Qty = x.qty,
                Rate = x.rate,
                Amount = x.qty * x.rate
            }).ToList();

            decimal total = _lines.Sum(x => x.qty * x.rate);
            lblTotal.Text = "Total: " + total.ToString("N2");
        }

        private void SaveSale()
        {
            if (_lines.Count == 0)
            {
                MessageBox.Show("No items.", "POS", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var header = new PHARMA.Models.Sale
            {
                invno = _invNo,
                invdt = DateTime.Now,
                Net = _lines.Sum(x => x.qty * x.rate),
                grsamt = _lines.Sum(x => x.qty * x.rate),
                type = 1,
                Posted = "Y",
                Operator = (AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : ""),
                Computername = Environment.MachineName,
                PostTime = DateTime.Now.ToString("HH:mm:ss")
            };

            string error;
            if (_saleService.SaveSale(header, _lines, out error))
            {
                MessageBox.Show("Sale saved. Invoice # " + _invNo, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NewSale();
            }
            else
            {
                MessageBox.Show("Save failed: " + error, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PosForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { NewSale(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { SaveSale(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { this.Close(); e.Handled = true; }
        }
    }
}
