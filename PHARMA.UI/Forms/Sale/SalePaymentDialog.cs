using System;
using System.Drawing;
using System.Windows.Forms;
using PHARMA.UI.Helpers;

namespace PHARMA.UI.Forms.Sale
{
    public class SalePaymentDialog : Form
    {
        private decimal _billAmount;
        private TextBox txtExtraDisc, txtRemarks, txtCash, txtNet, txtChange, txtBill;
        private Label lblBig;

        public decimal ExtraDiscount { get; private set; }
        public decimal CashReceived { get; private set; }
        public decimal CashReturn { get; private set; }
        public decimal FinalNet { get; private set; }
        public string Remarks { get; private set; }

        public SalePaymentDialog(decimal billAmount)
        {
            _billAmount = billAmount;
            Text = "Finalize Sale";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 340);
            BackColor = Color.FromArgb(250, 248, 240);
            KeyPreview = true;
            BuildUI();
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; Close(); }
                if (e.KeyCode == Keys.F5 || (e.KeyCode == Keys.Enter && !txtRemarks.Focused))
                {
                    if (Accept()) e.Handled = true;
                }
            };
        }

        private void BuildUI()
        {
            int y = 16;
            Action<string, Control> row = (label, c) =>
            {
                Controls.Add(new Label { Text = label, Location = new Point(24, y + 3), AutoSize = true, Font = new Font("Segoe UI", 9.5F) });
                c.Location = new Point(160, y);
                c.Width = 200;
                Controls.Add(c);
                y += 34;
            };

            txtBill = new TextBox { ReadOnly = true, Text = _billAmount.ToString("N2"), BackColor = Color.White, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            txtExtraDisc = new TextBox { Text = "0" };
            txtRemarks = new TextBox();
            txtNet = new TextBox { ReadOnly = true, BackColor = Color.White, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            txtCash = new TextBox { Text = _billAmount.ToString("0.##"), Font = new Font("Segoe UI", 12F, FontStyle.Bold) };
            txtChange = new TextBox { ReadOnly = true, BackColor = Color.White };

            row("Bill Amount", txtBill);
            row("Extra Discount", txtExtraDisc);
            row("Remarks", txtRemarks);
            row("Net Amount", txtNet);
            row("Cash Received", txtCash);
            row("Cash Return", txtChange);

            txtExtraDisc.TextChanged += (s, e) => Recalc();
            txtCash.TextChanged += (s, e) => Recalc();

            lblBig = new Label
            {
                Location = new Point(160, y + 4),
                Size = new Size(200, 40),
                Font = new Font("Consolas", 20F, FontStyle.Bold),
                ForeColor = Color.LimeGreen,
                BackColor = Color.Black,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "0"
            };
            Controls.Add(lblBig);
            y += 50;

            var btnOk = new Button { Text = "Save (F5)", Location = new Point(160, y), Size = new Size(100, 34) };
            UiStyle.StylePrimaryButton(btnOk);
            btnOk.Click += (s, e) => Accept();
            var btnCancel = new Button { Text = "Cancel", Location = new Point(270, y), Size = new Size(90, 34) };
            UiStyle.StyleSecondaryButton(btnCancel);
            btnCancel.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            Controls.Add(btnOk);
            Controls.Add(btnCancel);

            Recalc();
            Shown += (s, e) => { txtCash.Focus(); txtCash.SelectAll(); };
        }

        private void Recalc()
        {
            decimal disc = 0;
            decimal.TryParse(txtExtraDisc.Text, out disc);
            if (disc < 0) disc = 0;
            FinalNet = _billAmount - disc;
            if (FinalNet < 0) FinalNet = 0;
            txtNet.Text = FinalNet.ToString("N2");

            decimal cash = 0;
            decimal.TryParse(txtCash.Text, out cash);
            CashReturn = cash - FinalNet;
            txtChange.Text = CashReturn.ToString("N2");
            lblBig.Text = Math.Round(cash).ToString("0");
            if (CashReturn < 0)
                lblBig.ForeColor = Color.OrangeRed;
            else
                lblBig.ForeColor = Color.LimeGreen;
        }

        private bool Accept()
        {
            Recalc();
            decimal cash = 0;
            decimal.TryParse(txtCash.Text, out cash);
            if (cash < FinalNet)
            {
                var r = MessageBox.Show(
                    "Cash received is less than net amount. Continue anyway?",
                    "Payment",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return false;
            }
            decimal disc = 0;
            decimal.TryParse(txtExtraDisc.Text, out disc);
            ExtraDiscount = disc;
            CashReceived = cash;
            Remarks = txtRemarks.Text.Trim();
            DialogResult = DialogResult.OK;
            Close();
            return true;
        }
    }
}
