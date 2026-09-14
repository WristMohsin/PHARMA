using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;
using PHARMA.UI.Helpers;

namespace PHARMA.UI.Forms.Management
{
    public class ChartOfAccountsForm : Form
    {
        private readonly AccountService _svc = new AccountService();
        private TextBox txtSearch, txtCode, txtName, txtMain, txtScode, txtBalance, txtPhone, txtMobile;
        private ComboBox cboType;
        private CheckBox chkActive, chkSys;
        private DataGridView dgv;
        private Label lblInfo;
        private Button btnNew, btnSave, btnDeactivate, btnSearch, btnClose, btnRefresh;
        private bool _isNew;
        private bool _dirty;
        private int _loadedCode;

        public ChartOfAccountsForm()
        {
            Text = "Chart of Accounts";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Microsoft Sans Serif", 9F);
            FormClosing += ChartOfAccountsForm_FormClosing;
            BuildUI();
            ClearDetail(true);
            LoadData();
            KeyDown += ChartOfAccountsForm_KeyDown;
        }

        private void ChartOfAccountsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                if (_dirty && !UiStyle.ConfirmClose(this, "Chart of Accounts"))
                    e.Cancel = true;
            }
        }

        private void BuildUI()
        {
            var tool = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(245, 240, 225) };
            txtSearch = new TextBox { Location = new Point(8, 10), Size = new Size(220, 24), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { LoadData(); e.SuppressKeyPress = true; } };
            btnSearch = new Button { Text = "Search", Location = new Point(234, 8), Size = new Size(70, 28) };
            btnRefresh = new Button { Text = "Refresh", Location = new Point(308, 8), Size = new Size(70, 28) };
            btnNew = new Button { Text = "New (F2)", Location = new Point(400, 8), Size = new Size(78, 28) };
            btnSave = new Button { Text = "Save (F5)", Location = new Point(482, 8), Size = new Size(78, 28) };
            btnDeactivate = new Button { Text = "Deactivate", Location = new Point(564, 8), Size = new Size(88, 28) };
            btnClose = new Button { Text = "Close (Esc)", Location = new Point(656, 8), Size = new Size(84, 28) };
            btnSearch.Click += (s, e) => LoadData();
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); LoadData(); };
            btnNew.Click += (s, e) => StartNew();
            btnSave.Click += (s, e) => Save();
            btnDeactivate.Click += (s, e) => DeactivateSelected();
            btnClose.Click += (s, e) => Close();
            tool.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh, btnNew, btnSave, btnDeactivate, btnClose });

            var detail = new Panel { Dock = DockStyle.Bottom, Height = 160, BackColor = Color.FromArgb(255, 250, 235) };
            int y = 10;
            detail.Controls.Add(L("Code", 8, y + 2));
            txtCode = T(48, y, 70); detail.Controls.Add(txtCode);
            detail.Controls.Add(L("Name", 130, y + 2));
            txtName = T(170, y, 260); detail.Controls.Add(txtName);
            detail.Controls.Add(L("Type", 440, y + 2));
            cboType = new ComboBox { Location = new Point(480, y), Size = new Size(150, 24), DropDownStyle = ComboBoxStyle.DropDownList };
            cboType.Items.AddRange(new object[] { "C - Customer / AR", "S - Supplier / AP", "G - General", "B - Bank / Cash", "E - Expense", "I - Income", "O - Other" });
            cboType.SelectedIndex = 0; detail.Controls.Add(cboType);
            detail.Controls.Add(L("Active", 650, y + 2));
            chkActive = new CheckBox { Location = new Point(700, y), Checked = true, AutoSize = true, Text = "" }; detail.Controls.Add(chkActive);

            y = 44;
            detail.Controls.Add(L("Main", 8, y + 2));
            txtMain = T(48, y, 70); detail.Controls.Add(txtMain);
            detail.Controls.Add(L("S-Code", 130, y + 2));
            txtScode = T(180, y, 70); detail.Controls.Add(txtScode);
            detail.Controls.Add(L("Balance", 270, y + 2));
            txtBalance = T(325, y, 90); txtBalance.ReadOnly = true; txtBalance.BackColor = Color.WhiteSmoke; detail.Controls.Add(txtBalance);
            detail.Controls.Add(L("Phone", 430, y + 2));
            txtPhone = T(475, y, 100); detail.Controls.Add(txtPhone);
            detail.Controls.Add(L("Mobile", 590, y + 2));
            txtMobile = T(640, y, 100); detail.Controls.Add(txtMobile);

            y = 78;
            chkSys = new CheckBox { Location = new Point(8, y), AutoSize = true, Text = "System account (SysAc)" }; detail.Controls.Add(chkSys);
            lblInfo = new Label { Location = new Point(8, 110), AutoSize = true, ForeColor = Color.DimGray,
                Text = "F2 New  F5 Save  F3 Search  Esc Close  |  Double-click row to edit  |  Used accounts: deactivate only" };
            detail.Controls.Add(lblInfo);

            EventHandler mark = (s, e) => { _dirty = true; };
            foreach (var ctrl in new Control[] { txtCode, txtName, txtMain, txtScode, txtPhone, txtMobile })
            {
                var tb = ctrl as TextBox; if (tb != null) tb.TextChanged += mark;
            }
            cboType.SelectedIndexChanged += mark; chkActive.CheckedChanged += mark; chkSys.CheckedChanged += mark;

            dgv = new DataGridView {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.Fixed3D, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            UiStyle.StyleGrid(dgv);
            dgv.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) LoadFromGridRow(dgv.Rows[e.RowIndex]); };
            dgv.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter && dgv.CurrentRow != null) { LoadFromGridRow(dgv.CurrentRow); e.Handled = true; } };

            Controls.Add(dgv); Controls.Add(detail); Controls.Add(tool);
        }

        private static Label L(string t, int x, int y) { return new Label { Text = t, Location = new Point(x, y), AutoSize = true }; }
        private static TextBox T(int x, int y, int w) { return new TextBox { Location = new Point(x, y), Size = new Size(w, 22), BorderStyle = BorderStyle.FixedSingle }; }

        private string SelectedTypeCode()
        {
            if (cboType.SelectedItem == null) return "C";
            string s = cboType.SelectedItem.ToString();
            return s.Length > 0 ? s.Substring(0, 1).ToUpperInvariant() : "C";
        }

        private void SetTypeCombo(string code)
        {
            if (string.IsNullOrEmpty(code)) code = "C";
            code = code.Trim().ToUpperInvariant();
            for (int i = 0; i < cboType.Items.Count; i++)
            {
                if (cboType.Items[i].ToString().StartsWith(code, StringComparison.OrdinalIgnoreCase))
                { cboType.SelectedIndex = i; return; }
            }
            cboType.SelectedIndex = 0;
        }

        private static string TypeLabel(string code)
        {
            if (string.IsNullOrEmpty(code)) return "";
            switch (code.Trim().ToUpperInvariant())
            {
                case "C": return "Customer/AR";
                case "S": return "Supplier/AP";
                case "G": return "General";
                case "B": return "Bank/Cash";
                case "E": return "Expense";
                case "I": return "Income";
                case "O": return "Other";
                default: return code;
            }
        }

        private void LoadData()
        {
            try
            {
                var list = _svc.Search(txtSearch.Text.Trim());
                dgv.DataSource = null;
                dgv.DataSource = list.Select(a => new {
                    Code = a.acno,
                    Name = !string.IsNullOrEmpty(a.NAME) ? a.NAME : a.dsc,
                    Type = TypeLabel(a.Partytype),
                    TypeCode = a.Partytype,
                    Main = a.Main,
                    SCode = a.Scode,
                    Balance = a.Balance,
                    Active = string.Equals(a.StopTrans, "Y", StringComparison.OrdinalIgnoreCase) ? "No" : "Yes",
                    Sys = a.SysAc
                }).ToList();
                lblInfo.Text = "Accounts: " + list.Count + "  |  F2 New  F5 Save  F3 Search  Esc Close";
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ChartOfAccountsForm.LoadData: " + ex.Message);
                MessageBox.Show("Could not load accounts. Check the database connection.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadFromGridRow(DataGridViewRow row)
        {
            if (row == null || row.Cells["Code"].Value == null) return;
            int code; if (!int.TryParse(row.Cells["Code"].Value.ToString(), out code)) return;
            try
            {
                var a = _svc.Get(code);
                if (a == null) { MessageBox.Show("Account not found.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
                _isNew = false; _loadedCode = a.acno;
                txtCode.Text = a.acno.ToString(); txtCode.ReadOnly = true; txtCode.BackColor = Color.WhiteSmoke;
                txtName.Text = !string.IsNullOrEmpty(a.NAME) ? a.NAME : (a.dsc ?? "");
                SetTypeCombo(a.Partytype);
                txtMain.Text = a.Main ?? ""; txtScode.Text = a.Scode.ToString();
                txtBalance.Text = a.Balance.ToString("N2");
                txtPhone.Text = a.Phone ?? ""; txtMobile.Text = a.Mobile ?? "";
                chkActive.Checked = !string.Equals(a.StopTrans, "Y", StringComparison.OrdinalIgnoreCase);
                chkSys.Checked = !string.IsNullOrWhiteSpace(a.SysAc) && a.SysAc.Trim() != "";
                _dirty = false; txtName.Focus();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ChartOfAccountsForm.LoadFromGridRow: " + ex.Message);
                MessageBox.Show("Could not load account details.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void StartNew()
        {
            if (_dirty)
            {
                var r = MessageBox.Show("Discard unsaved changes?", "Chart of Accounts", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
            }
            ClearDetail(true);
            try { txtCode.Text = _svc.NextCode().ToString(); }
            catch (Exception ex) { Trace.WriteLine("ChartOfAccountsForm.NextCode: " + ex.Message); txtCode.Text = ""; }
            txtCode.Focus();
        }

        private void ClearDetail(bool asNew)
        {
            _isNew = asNew; _loadedCode = 0;
            txtCode.ReadOnly = false; txtCode.BackColor = Color.White; txtCode.Clear();
            txtName.Clear(); txtMain.Clear(); txtScode.Text = "0"; txtBalance.Text = "0.00";
            txtPhone.Clear(); txtMobile.Clear(); cboType.SelectedIndex = 0;
            chkActive.Checked = true; chkSys.Checked = false; _dirty = false;
        }

        private void Save()
        {
            int code;
            if (!int.TryParse(txtCode.Text.Trim(), out code) || code <= 0)
            { MessageBox.Show("Account code must be a positive whole number.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtCode.Focus(); return; }
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            { MessageBox.Show("Account name is required.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtName.Focus(); return; }
            if (name.Length > 50)
            { MessageBox.Show("Account name cannot exceed 50 characters.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            int scode = 0;
            if (!string.IsNullOrWhiteSpace(txtScode.Text) && !int.TryParse(txtScode.Text.Trim(), out scode))
            { MessageBox.Show("S-Code must be a whole number.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtScode.Focus(); return; }
            if (_isNew || code != _loadedCode)
            {
                try
                {
                    if (_svc.Get(code) != null)
                    { MessageBox.Show("Account code already exists: " + code, "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtCode.Focus(); return; }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("ChartOfAccountsForm.Save unique: " + ex.Message);
                    MessageBox.Show("Could not verify account code uniqueness.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
                }
            }
            decimal opening = 0;
            if (_isNew && !string.IsNullOrWhiteSpace(txtBalance.Text))
                decimal.TryParse(txtBalance.Text.Replace(",", ""), out opening);
            var a = new Account {
                acno = code, NAME = name, dsc = name, Partytype = SelectedTypeCode(),
                Main = string.IsNullOrWhiteSpace(txtMain.Text) ? null : txtMain.Text.Trim(),
                Scode = scode,
                Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text.Trim(),
                Mobile = string.IsNullOrWhiteSpace(txtMobile.Text) ? null : txtMobile.Text.Trim(),
                StopTrans = chkActive.Checked ? "N" : "Y",
                SysAc = chkSys.Checked ? "Y" : " ",
                Balance = opening
            };
            string error;
            if (!_svc.Save(a, out error))
            {
                MessageBox.Show(string.IsNullOrEmpty(error) ? "Save failed." : ("Could not save account. " + error),
                    "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }
            MessageBox.Show("Account saved: " + code + " \u2014 " + name, "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _dirty = false; _isNew = false; _loadedCode = code;
            txtCode.ReadOnly = true; txtCode.BackColor = Color.WhiteSmoke; LoadData();
        }

        private void DeactivateSelected()
        {
            int code = _loadedCode;
            if (code <= 0 && dgv.CurrentRow != null && dgv.CurrentRow.Cells["Code"].Value != null)
                int.TryParse(dgv.CurrentRow.Cells["Code"].Value.ToString(), out code);
            if (code <= 0)
            { MessageBox.Show("Select an account first.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            try
            {
                if (_svc.IsReferenced(code))
                {
                    var r = MessageBox.Show("This account is already used in transactions and cannot be deleted.\n\nDeactivate it instead (StopTrans = Y)?",
                        "Chart of Accounts", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                    if (r != DialogResult.Yes) return;
                }
                else
                {
                    var r = MessageBox.Show("Deactivate account " + code + "? It will be blocked from future transactions.",
                        "Chart of Accounts", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (r != DialogResult.Yes) return;
                }
                string error;
                if (!_svc.Deactivate(code, out error))
                { MessageBox.Show(error ?? "Deactivate failed.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Error); return; }
                MessageBox.Show("Account deactivated: " + code, "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _dirty = false; LoadData(); ClearDetail(true);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ChartOfAccountsForm.Deactivate: " + ex.Message);
                MessageBox.Show("Could not deactivate account.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ChartOfAccountsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { StartNew(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { Save(); e.Handled = true; }
            else if (e.KeyCode == Keys.F3) { txtSearch.Focus(); txtSearch.SelectAll(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
        }
    }
}
