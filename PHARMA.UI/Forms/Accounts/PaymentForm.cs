using System;
using System.Drawing;
using System.Windows.Forms;
using PHARMA.Services;

namespace PHARMA.UI.Forms.Accounts
{
    public class PaymentForm : Form
    {
        private readonly AccountService _svc = new AccountService();
        private TextBox txtCode, txtAmount, txtRemarks;
        private Label lblName, lblBal;
        private ComboBox cmbType;

        public PaymentForm()
        {
            Text = "Payment / Receipt";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 280);
            KeyPreview = true;
            BuildUI();
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); if (e.KeyCode == Keys.F5) Save(); };
        }

        private void BuildUI()
        {
            int y = 20;
            Controls.Add(new Label { Text = "Account Code", Location = new Point(20, y), AutoSize = true });
            txtCode = new TextBox { Location = new Point(140, y - 2), Width = 100 };
            txtCode.Leave += LoadAccount;
            Controls.Add(txtCode);
            y += 32;
            lblName = new Label { Text = "", Location = new Point(140, y), AutoSize = true, ForeColor = Color.DarkGreen };
            Controls.Add(lblName);
            y += 28;
            lblBal = new Label { Text = "Balance: -", Location = new Point(140, y), AutoSize = true };
            Controls.Add(lblBal);
            y += 32;
            Controls.Add(new Label { Text = "Type", Location = new Point(20, y), AutoSize = true });
            cmbType = new ComboBox { Location = new Point(140, y - 2), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbType.Items.Add("Receipt (from party)");
            cmbType.Items.Add("Payment (to party)");
            cmbType.SelectedIndex = 0;
            Controls.Add(cmbType);
            y += 36;
            Controls.Add(new Label { Text = "Amount", Location = new Point(20, y), AutoSize = true });
            txtAmount = new TextBox { Location = new Point(140, y - 2), Width = 120 };
            Controls.Add(txtAmount);
            y += 36;
            Controls.Add(new Label { Text = "Remarks", Location = new Point(20, y), AutoSize = true });
            txtRemarks = new TextBox { Location = new Point(140, y - 2), Width = 240 };
            Controls.Add(txtRemarks);
            y += 50;
            var btnSave = new Button { Text = "Save (F5)", Location = new Point(140, y), Size = new Size(100, 32) };
            btnSave.Click += (s, e) => Save();
            var btnClose = new Button { Text = "Close", Location = new Point(250, y), Size = new Size(90, 32) };
            btnClose.Click += (s, e) => Close();
            Controls.Add(btnSave);
            Controls.Add(btnClose);
        }

        private void LoadAccount(object sender, EventArgs e)
        {
            int code = 0;
            int.TryParse(txtCode.Text, out code);
            if (code <= 0) return;
            try
            {
                var a = _svc.Get(code);
                if (a != null)
                {
                    lblName.Text = a.NAME != null ? a.NAME : a.dsc;
                    lblBal.Text = "Balance: " + a.Balance.ToString("N2");
                }
                else
                {
                    lblName.Text = "(not found)";
                    lblBal.Text = "Balance: -";
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
        }

        private void Save()
        {
            int code = 0;
            int.TryParse(txtCode.Text, out code);
            decimal amt = 0;
            decimal.TryParse(txtAmount.Text, out amt);
            if (code <= 0 || amt <= 0)
            {
                MessageBox.Show("Enter valid account and amount");
                return;
            }
            decimal delta = cmbType.SelectedIndex == 0 ? -amt : amt;
            string err;
            if (_svc.AdjustBalance(code, delta, out err))
            {
                MessageBox.Show("Saved.");
                LoadAccount(null, null);
                txtAmount.Clear();
            }
            else MessageBox.Show("Error: " + err);
        }
    }
}
