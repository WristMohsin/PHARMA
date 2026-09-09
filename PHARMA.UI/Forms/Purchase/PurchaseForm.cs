using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;

namespace PHARMA.UI.Forms.Purchase
{
    public class PurchaseForm : Form
    {
        private readonly PurchaseService _svc = new PurchaseService();
        private List<pur_det> _lines = new List<pur_det>();
        private int _invNo;
        private TextBox txtCode, txtParty;
        private DataGridView dgv;
        private Label lblInv, lblTotal;

        public PurchaseForm()
        {
            Text = "Purchase Entry";
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            BuildUI();
            NewDoc();
            KeyDown += Form_KeyDown;
        }

        private void BuildUI()
        {
            lblInv = new Label { Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            var lblParty = new Label { Text = "Supplier Code", Location = new Point(20, 50), AutoSize = true };
            txtParty = new TextBox { Location = new Point(130, 47), Size = new Size(100, 25) };
            txtCode = new TextBox { Location = new Point(20, 85), Size = new Size(300, 28), Font = new Font("Consolas", 12F) };
            txtCode.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    AddItem(txtCode.Text.Trim());
                    txtCode.Clear();
                    e.SuppressKeyPress = true;
                }
            };
            var lblHint = new Label { Text = "Product code/barcode + Enter  |  F2=New  F5=Save  Esc=Close", Location = new Point(340, 88), AutoSize = true, ForeColor = Color.DarkBlue };

            dgv = new DataGridView
            {
                Location = new Point(20, 125),
                Size = new Size(900, 380),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            lblTotal = new Label { Location = new Point(700, 520), AutoSize = true, Font = new Font("Segoe UI", 14F, FontStyle.Bold), Anchor = AnchorStyles.Bottom | AnchorStyles.Right };

            var btnNew = new Button { Text = "New (F2)", Location = new Point(20, 515), Size = new Size(90, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnNew.Click += (s, e) => NewDoc();
            var btnSave = new Button { Text = "Save (F5)", Location = new Point(120, 515), Size = new Size(90, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnSave.Click += (s, e) => Save();
            var btnClose = new Button { Text = "Close (Esc)", Location = new Point(220, 515), Size = new Size(100, 35), Anchor = AnchorStyles.Bottom | AnchorStyles.Left };
            btnClose.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { lblInv, lblParty, txtParty, txtCode, lblHint, dgv, lblTotal, btnNew, btnSave, btnClose });
        }

        private void NewDoc()
        {
            _lines.Clear();
            _invNo = _svc.NextInvNo();
            lblInv.Text = "Purchase Invoice #: " + _invNo;
            txtParty.Text = "0";
            RefreshGrid();
            txtCode.Focus();
        }

        private void AddItem(string code)
        {
            if (string.IsNullOrEmpty(code)) return;
            var p = _svc.FindProduct(code);
            if (p == null) { MessageBox.Show("Product not found: " + code); return; }
            var existing = _lines.FirstOrDefault(x => x.pcode == p.pcode);
            if (existing != null) existing.qty += 1;
            else
            {
                _lines.Add(new pur_det
                {
                    pcode = p.pcode,
                    rate = p.Pur_Rate > 0 ? p.Pur_Rate : p.tp,
                    qty = 1,
                    bonus = 0,
                    SortNo = _lines.Count + 1
                });
            }
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            dgv.DataSource = _lines.Select(x => new { Code = x.pcode, Qty = x.qty, Rate = x.rate, Amount = x.qty * x.rate }).ToList();
            lblTotal.Text = "Total: " + _lines.Sum(x => x.qty * x.rate).ToString("N2");
        }

        private void Save()
        {
            if (_lines.Count == 0) { MessageBox.Show("No items."); return; }
            int party = 0;
            int.TryParse(txtParty.Text, out party);
            var header = new purchase
            {
                invno = _invNo,
                invdt = DateTime.Now,
                code = party,
                grsamt = _lines.Sum(x => x.qty * x.rate),
                net = _lines.Sum(x => x.qty * x.rate),
                type = 1,
                Operator = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "",
                ComputerName = Environment.MachineName
            };
            string err;
            if (_svc.Save(header, _lines, out err))
            {
                MessageBox.Show("Purchase saved. Inv # " + _invNo);
                NewDoc();
            }
            else MessageBox.Show("Error: " + err);
        }

        private void Form_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { NewDoc(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { Save(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
        }
    }
}
