using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;

namespace PHARMA.UI.Forms.Management
{
    public class AccountEditorForm : Form
    {
        private readonly AccountService _svc = new AccountService();
        private readonly bool _isNew;
        private readonly int _editCode;

        private TextBox txtCode, txtName, txtMain, txtScode, txtBalance, txtPhone, txtMobile;
        private ComboBox cboType;
        private CheckBox chkActive, chkSys;
        private Button btnSave, btnCancel;
        private Label lblError;
        private bool _dirty;

        public int SavedCode { get; private set; }

        public static AccountEditorForm ForNew()
        {
            return new AccountEditorForm(true, 0, null);
        }

        public static AccountEditorForm ForEdit(Account existing)
        {
            if (existing == null) throw new ArgumentNullException("existing");
            return new AccountEditorForm(false, existing.acno, existing);
        }

        private AccountEditorForm(bool isNew, int editCode, Account existing)
        {
            _isNew = isNew;
            _editCode = editCode;
            Text = isNew ? "New Account" : "Edit Account";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(420, 340);
            KeyPreview = true;
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Microsoft Sans Serif", 9F);
            BuildUI();
            if (isNew)
                LoadNewDefaults();
            else
                LoadExisting(existing);
            KeyDown += AccountEditorForm_KeyDown;
            FormClosing += AccountEditorForm_FormClosing;
        }

        private void BuildUI()
        {
            int y = 14;
            Controls.Add(L("Code", 16, y + 2));
            txtCode = Tb(110, y, 100);
            Controls.Add(txtCode);

            y += 30;
            Controls.Add(L("Name", 16, y + 2));
            txtName = Tb(110, y, 280);
            Controls.Add(txtName);

            y += 30;
            Controls.Add(L("Type", 16, y + 2));
            cboType = new ComboBox
            {
                Location = new Point(110, y),
                Size = new Size(100, 24),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboType.Items.AddRange(new object[] { "C", "S", "G", "B", "E", "I", "O" });
            cboType.SelectedIndex = 0;
            Controls.Add(cboType);

            y += 30;
            Controls.Add(L("Main", 16, y + 2));
            txtMain = Tb(110, y, 80);
            Controls.Add(txtMain);

            Controls.Add(L("S-Code", 210, y + 2));
            txtScode = Tb(270, y, 80);
            Controls.Add(txtScode);

            y += 30;
            Controls.Add(L(_isNew ? "Opening Bal." : "Balance", 16, y + 2));
            txtBalance = Tb(110, y, 120);
            Controls.Add(txtBalance);

            y += 30;
            Controls.Add(L("Phone", 16, y + 2));
            txtPhone = Tb(110, y, 120);
            Controls.Add(txtPhone);

            Controls.Add(L("Mobile", 240, y + 2));
            txtMobile = Tb(290, y, 100);
            Controls.Add(txtMobile);

            y += 32;
            chkActive = new CheckBox { Text = "Active", Location = new Point(110, y), AutoSize = true, Checked = true };
            Controls.Add(chkActive);
            chkSys = new CheckBox { Text = "System account", Location = new Point(210, y), AutoSize = true };
            Controls.Add(chkSys);

            y += 36;
            lblError = new Label { Location = new Point(16, y), Size = new Size(380, 32), ForeColor = Color.DarkRed, Text = "" };
            Controls.Add(lblError);

            btnSave = new Button { Text = "Save (F5)", Location = new Point(210, 300), Size = new Size(90, 28) };
            btnCancel = new Button { Text = "Cancel (Esc)", Location = new Point(310, 300), Size = new Size(90, 28) };
            btnSave.Click += (s, e) => Save();
            btnCancel.Click += (s, e) => TryCancel();
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            AcceptButton = null;
            CancelButton = btnCancel;

            EventHandler mark = (s, e) => { _dirty = true; ClearError(); };
            txtCode.TextChanged += mark;
            txtName.TextChanged += mark;
            txtMain.TextChanged += mark;
            txtScode.TextChanged += mark;
            txtBalance.TextChanged += mark;
            txtPhone.TextChanged += mark;
            txtMobile.TextChanged += mark;
            cboType.SelectedIndexChanged += mark;
            chkActive.CheckedChanged += mark;
            chkSys.CheckedChanged += mark;

            foreach (Control c in new Control[] { txtCode, txtName, txtMain, txtScode, txtBalance, txtPhone, txtMobile })
            {
                var tb = c as TextBox;
                if (tb != null) tb.KeyDown += Field_KeyDown;
            }
        }

        private static Label L(string text, int x, int y)
        {
            return new Label { Text = text, Location = new Point(x, y), AutoSize = true };
        }

        private static TextBox Tb(int x, int y, int w)
        {
            return new TextBox { Location = new Point(x, y), Size = new Size(w, 22), BorderStyle = BorderStyle.FixedSingle };
        }

        private void LoadNewDefaults()
        {
            txtCode.ReadOnly = false;
            txtCode.BackColor = Color.White;
            try { txtCode.Text = _svc.NextCode().ToString(); }
            catch (Exception ex)
            {
                Trace.WriteLine("AccountEditorForm.NextCode: " + ex.Message);
                txtCode.Text = "";
            }
            txtName.Clear();
            cboType.SelectedIndex = 0;
            txtMain.Clear();
            txtScode.Text = "0";
            txtBalance.ReadOnly = false;
            txtBalance.BackColor = Color.White;
            txtBalance.Text = "0.00";
            txtPhone.Clear();
            txtMobile.Clear();
            chkActive.Checked = true;
            chkSys.Checked = false;
            _dirty = false;
            BeginInvoke(new Action(delegate { txtName.Focus(); }));
        }

        private void LoadExisting(Account a)
        {
            txtCode.Text = a.acno.ToString();
            txtCode.ReadOnly = true;
            txtCode.BackColor = Color.WhiteSmoke;
            txtName.Text = !string.IsNullOrEmpty(a.NAME) ? a.NAME : (a.dsc ?? "");
            SetType(a.Partytype);
            txtMain.Text = a.Main ?? "";
            txtScode.Text = a.Scode.ToString();
            txtBalance.Text = a.Balance.ToString("N2");
            txtBalance.ReadOnly = true;
            txtBalance.BackColor = Color.WhiteSmoke;
            txtPhone.Text = a.Phone ?? "";
            txtMobile.Text = a.Mobile ?? "";
            chkActive.Checked = !string.Equals(a.StopTrans, "Y", StringComparison.OrdinalIgnoreCase);
            chkSys.Checked = !string.IsNullOrWhiteSpace(a.SysAc) && a.SysAc.Trim() != "";
            _dirty = false;
            BeginInvoke(new Action(delegate { txtName.Focus(); }));
        }

        private void SetType(string code)
        {
            if (string.IsNullOrEmpty(code)) code = "C";
            code = code.Trim();
            if (code.Length > 1) code = code.Substring(0, 1);
            code = code.ToUpperInvariant();
            for (int i = 0; i < cboType.Items.Count; i++)
            {
                if (string.Equals(cboType.Items[i].ToString(), code, StringComparison.OrdinalIgnoreCase))
                {
                    cboType.SelectedIndex = i;
                    return;
                }
            }
            cboType.Items.Add(code);
            cboType.SelectedItem = code;
        }

        private void Field_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void AccountEditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5) { Save(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.Enter) { Save(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { TryCancel(); e.Handled = true; }
        }

        private void AccountEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK) return;
            if (_dirty)
            {
                var r = MessageBox.Show("Discard unsaved changes?", Text, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) e.Cancel = true;
            }
        }

        private void TryCancel()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void ClearError() { lblError.Text = ""; }

        private void ShowError(string msg, Control focus)
        {
            lblError.Text = msg;
            if (focus != null) focus.Focus();
        }

        private void Save()
        {
            ClearError();
            int code;
            if (!int.TryParse(txtCode.Text.Trim(), out code) || code <= 0)
            { ShowError("Account code must be a positive whole number.", txtCode); return; }
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name)) { ShowError("Account name is required.", txtName); return; }
            if (name.Length > 50) { ShowError("Account name cannot exceed 50 characters.", txtName); return; }
            string type = cboType.SelectedItem != null ? cboType.SelectedItem.ToString().Trim() : "C";
            if (type.Length == 0) type = "C";
            if (type.Length > 1) type = type.Substring(0, 1);
            string main = txtMain.Text.Trim();
            if (main.Length > 6) { ShowError("Main cannot exceed 6 characters.", txtMain); return; }
            int scode = 0;
            if (!string.IsNullOrWhiteSpace(txtScode.Text) && !int.TryParse(txtScode.Text.Trim(), out scode))
            { ShowError("S-Code must be a whole number.", txtScode); return; }
            decimal opening = 0;
            if (_isNew)
            {
                if (!string.IsNullOrWhiteSpace(txtBalance.Text) &&
                    !decimal.TryParse(txtBalance.Text.Trim().Replace(",", ""), out opening))
                { ShowError("Opening balance must be numeric.", txtBalance); return; }
            }
            string phone = txtPhone.Text.Trim();
            string mobile = txtMobile.Text.Trim();
            if (phone.Length > 25) { ShowError("Phone cannot exceed 25 characters.", txtPhone); return; }
            if (mobile.Length > 50) { ShowError("Mobile cannot exceed 50 characters.", txtMobile); return; }

            if (_isNew || code != _editCode)
            {
                try
                {
                    Account existing = _svc.Get(code);
                    if (existing != null) { ShowError("Account code already exists: " + code, txtCode); return; }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("AccountEditorForm.Save unique: " + ex.Message);
                    ShowError("Could not verify account code uniqueness.", txtCode);
                    return;
                }
            }

            var a = new Account
            {
                acno = code,
                NAME = name,
                dsc = name,
                Partytype = type,
                Main = string.IsNullOrEmpty(main) ? null : main,
                Scode = scode,
                Phone = string.IsNullOrEmpty(phone) ? null : phone,
                Mobile = string.IsNullOrEmpty(mobile) ? null : mobile,
                StopTrans = chkActive.Checked ? "N" : "Y",
                SysAc = chkSys.Checked ? "Y" : " ",
                Balance = opening
            };

            string error;
            if (!_svc.Save(a, out error))
            {
                ShowError(string.IsNullOrEmpty(error) ? "Save failed." : error, null);
                return;
            }

            SavedCode = code;
            _dirty = false;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
