using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;

namespace PHARMA.UI.Forms.Sale
{
    public class SaleReturnForm : Form
    {
        private readonly SaleService _svc = new SaleService();
        private List<Sale_Detail> _lines = new List<Sale_Detail>();
        private int _invNo;
        private TextBox txtCode, txtParty;
        private DataGridView dgv;
        private Label lblInv, lblTotal;

        public SaleReturnForm()
        {
            Text = "Sale Return";
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            BuildUI();
            NewDoc();
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F2) NewDoc();
                else if (e.KeyCode == Keys.F5) Save();
                else if (e.KeyCode == Keys.Escape) Close();
            };
        }

        private void BuildUI()
        {
            lblInv = new Label { Location = new Point(20, 15), AutoSize = true, Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            var lblP = new Label { Text = "Party", Location = new Point(20, 50), AutoSize = true };
            txtParty = new TextBox { Location = new Point(80, 47), Size = new Size(80, 25), Text = "0" };
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
            var hint = new Label { Text = "Product + Enter | F2=New F5=Save Esc=Close", Location = new Point(340, 88), AutoSize = true, ForeColor = Color.DarkBlue };
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
            Controls.AddRange(new Control[] { lblInv, lblP, txtParty, txtCode, hint, dgv, lblTotal, btnNew, btnSave, btnClose });
        }

        private void NewDoc()
        {
            _lines.Clear();
            _invNo = _svc.GetNextInvNo();
            lblInv.Text = "Return Invoice #: " + _invNo;
            txtParty.Text = "0";
            RefreshGrid();
            txtCode.Focus();
        }

        private void AddItem(string code)
        {
            if (string.IsNullOrEmpty(code)) return;
            var p = _svc.FindProduct(code);
            if (p == null) { MessageBox.Show("Product not found"); return; }
            var existing = _lines.FirstOrDefault(x => x.pcode == p.pcode);
            if (existing != null) existing.qty += 1;
            else
            {
                var d = new Sale_Detail();
                d.pcode = p.pcode;
                d.rate = p.rp > 0 ? p.rp : p.tp;
                d.qty = 1;
                d.SortNo = _lines.Count + 1;
                _lines.Add(d);
            }
            RefreshGrid();
        }

        private void RefreshGrid()
        {
            dgv.DataSource = _lines.Select(x => new { Code = x.pcode, Qty = x.qty, Rate = x.rate, Amount = x.qty * x.rate }).ToList();
            decimal t = 0;
            foreach (var x in _lines) t += x.qty * x.rate;
            lblTotal.Text = "Return Total: " + t.ToString("N2");
        }

        private void Save()
        {
            if (_lines.Count == 0) { MessageBox.Show("No items"); return; }
            int party = 0;
            int.TryParse(txtParty.Text, out party);
            decimal gross = 0;
            foreach (var x in _lines) gross += x.qty * x.rate;
            var header = new PHARMA.Models.Sale();
            header.invno = _invNo;
            header.invdt = DateTime.Now;
            header.code = party;
            header.grsamt = gross;
            header.Net = gross;
            header.type = 2;
            header.Posted = "Y";
            header.Operator = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "";
            header.Computername = Environment.MachineName;
            header.PostTime = DateTime.Now.ToString("HH:mm:ss");
            header.Remarks = "RETURN";
            string err;
            if (_svc.SaveReturn(header, _lines, out err))
            {
                MessageBox.Show("Return saved. # " + _invNo);
                NewDoc();
            }
            else MessageBox.Show("Error: " + err);
        }
    }
}
