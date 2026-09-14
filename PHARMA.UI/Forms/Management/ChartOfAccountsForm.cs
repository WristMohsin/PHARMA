using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly BindingList<CoaRow> _rows = new BindingList<CoaRow>();
        private TextBox txtSearch;
        private DataGridView dgv;
        private Label lblStatus;
        private Button btnNew, btnSave, btnDeactivate, btnSearch, btnRefresh, btnClose;
        private bool _loading;

        public ChartOfAccountsForm()
        {
            Text = "Chart of Accounts";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Microsoft Sans Serif", 9F);
            FormClosing += ChartOfAccountsForm_FormClosing;
            BuildUI();
            ReloadFromDb();
            KeyDown += ChartOfAccountsForm_KeyDown;
        }

        private void ChartOfAccountsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                if (HasPending() && !ConfirmDiscard("close Chart of Accounts"))
                    e.Cancel = true;
            }
        }

        private void BuildUI()
        {
            var tool = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(245, 240, 225) };
            txtSearch = new TextBox { Location = new Point(6, 8), Size = new Size(200, 22), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.KeyDown += TxtSearch_KeyDown;
            btnSearch = MkBtn("Search", 212, 6, 64);
            btnRefresh = MkBtn("Refresh", 280, 6, 64);
            btnNew = MkBtn("New (F2)", 360, 6, 72);
            btnSave = MkBtn("Save (F5)", 436, 6, 72);
            btnDeactivate = MkBtn("Deactivate", 512, 6, 80);
            btnClose = MkBtn("Close (Esc)", 596, 6, 80);
            btnSearch.Click += (s, e) => SearchOrWarn();
            btnRefresh.Click += (s, e) => RefreshOrWarn();
            btnNew.Click += (s, e) => AddNewRow();
            btnSave.Click += (s, e) => SaveAll();
            btnDeactivate.Click += (s, e) => DeactivateCurrent();
            btnClose.Click += (s, e) => Close();
            tool.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh, btnNew, btnSave, btnDeactivate, btnClose });

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(245, 240, 225) };
            lblStatus = new Label {
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0),
                ForeColor = Color.DimGray,
                Text = "F2 New row  F5 Save all  F3 Search  Esc  |  Type = Partytype code as stored in DB"
            };
            footer.Controls.Add(lblStatus);

            dgv = new DataGridView {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.Fixed3D,
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2, AutoGenerateColumns = false, StandardTab = true
            };
            dgv.RowTemplate.Height = 22;
            dgv.ColumnHeadersHeight = 24;
            UiStyle.StyleGrid(dgv);
            BuildColumns();
            dgv.DataSource = _rows;
            dgv.CellBeginEdit += Dgv_CellBeginEdit;
            dgv.CellValidating += Dgv_CellValidating;
            dgv.CellEndEdit += Dgv_CellEndEdit;
            dgv.CellValueChanged += Dgv_CellValueChanged;
            dgv.DataError += (s, e) => { e.ThrowException = false; };
            dgv.KeyDown += Dgv_KeyDown;

            Controls.Add(dgv);
            Controls.Add(footer);
            Controls.Add(tool);
        }

        private static Button MkBtn(string text, int x, int y, int w)
        {
            return new Button { Text = text, Location = new Point(x, y), Size = new Size(w, 26), FlatStyle = FlatStyle.System };
        }

        private void BuildColumns()
        {
            dgv.Columns.Clear();
            dgv.Columns.Add(ColText("Code", "Code", 70, false));
            dgv.Columns.Add(ColText("Name", "Account Name", 220, false));
            var typeCol = new DataGridViewComboBoxColumn {
                Name = "Type", DataPropertyName = "Type", HeaderText = "Type", Width = 70, FlatStyle = FlatStyle.Flat
            };
            typeCol.Items.AddRange(new object[] { "C", "S", "G", "B", "E", "I", "O", "" });
            dgv.Columns.Add(typeCol);
            dgv.Columns.Add(ColText("Main", "Main", 60, false));
            dgv.Columns.Add(ColText("SCode", "S-Code", 60, false));
            var bal = ColText("Balance", "Balance", 90, true);
            bal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            bal.DefaultCellStyle.Format = "N2";
            dgv.Columns.Add(bal);
            dgv.Columns.Add(ColCheck("Active", "Active", 55));
            dgv.Columns.Add(ColCheck("Sys", "Sys", 40));
            dgv.Columns.Add(ColText("Status", "Status", 70, true));
        }

        private static DataGridViewTextBoxColumn ColText(string prop, string header, int width, bool readOnly)
        {
            return new DataGridViewTextBoxColumn {
                Name = prop, DataPropertyName = prop, HeaderText = header, Width = width, ReadOnly = readOnly,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private static DataGridViewCheckBoxColumn ColCheck(string prop, string header, int width)
        {
            return new DataGridViewCheckBoxColumn {
                Name = prop, DataPropertyName = prop, HeaderText = header, Width = width,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private bool HasPending()
        {
            foreach (CoaRow r in _rows)
                if (r.IsNew || r.IsDirty) return true;
            return false;
        }

        private int PendingCount()
        {
            int n = 0;
            foreach (CoaRow r in _rows)
                if (r.IsNew || r.IsDirty) n++;
            return n;
        }

        private bool ConfirmDiscard(string action)
        {
            var r = MessageBox.Show(
                "You have " + PendingCount() + " unsaved account change(s).\nDiscard and " + action + "?",
                "Chart of Accounts", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            return r == DialogResult.Yes;
        }

        private void UpdateStatus(string extra)
        {
            int pend = PendingCount();
            lblStatus.Text = "Rows: " + _rows.Count + "  |  Pending: " + pend +
                "  |  F2 New  F5 Save all  F3 Search  Esc  |  Type = Partytype code as stored in DB" +
                (string.IsNullOrEmpty(extra) ? "" : ("  |  " + extra));
        }

        private void ReloadFromDb() { ReloadFromDb(txtSearch != null ? txtSearch.Text.Trim() : ""); }

        private void ReloadFromDb(string term)
        {
            _loading = true;
            try
            {
                _rows.Clear();
                List<Account> list = _svc.Search(term);
                foreach (Account a in list)
                    _rows.Add(CoaRow.FromAccount(a));
                UpdateStatus("Loaded");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ChartOfAccountsForm.ReloadFromDb: " + ex.Message);
                MessageBox.Show("Could not load accounts. Check the database connection.",
                    "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally { _loading = false; }
        }

        private void SearchOrWarn()
        {
            if (HasPending() && !ConfirmDiscard("search")) return;
            ReloadFromDb(txtSearch.Text.Trim());
        }

        private void RefreshOrWarn()
        {
            if (HasPending() && !ConfirmDiscard("refresh")) return;
            txtSearch.Clear();
            ReloadFromDb("");
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { SearchOrWarn(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) { dgv.Focus(); e.Handled = true; }
        }

        private void AddNewRow()
        {
            if (dgv.IsCurrentCellInEditMode) dgv.EndEdit();
            int next = 1;
            try { next = _svc.NextCode(); }
            catch (Exception ex) { Trace.WriteLine("ChartOfAccountsForm.NextCode: " + ex.Message); }
            foreach (CoaRow r in _rows)
                if (r.Code >= next) next = r.Code + 1;

            var row = new CoaRow {
                IsNew = true, IsDirty = true, Code = next, Name = "", Type = "C", Main = "",
                SCode = 0, Balance = 0, Active = true, Sys = false, Status = "New"
            };
            _rows.Add(row);
            int idx = _rows.Count - 1;
            dgv.Focus();
            if (idx >= 0 && idx < dgv.Rows.Count)
            {
                dgv.CurrentCell = dgv.Rows[idx].Cells["Name"];
                dgv.BeginEdit(true);
            }
            UpdateStatus("New row");
        }

        private void Dgv_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _rows.Count) return;
            CoaRow row = _rows[e.RowIndex];
            string col = dgv.Columns[e.ColumnIndex].Name;
            if (col == "Code" && !row.IsNew) e.Cancel = true;
            if (col == "Balance" && !row.IsNew) e.Cancel = true;
            if (col == "Status") e.Cancel = true;
        }

        private void Dgv_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_loading || e.RowIndex < 0 || e.RowIndex >= _rows.Count) return;
            string col = dgv.Columns[e.ColumnIndex].Name;
            string text = e.FormattedValue != null ? e.FormattedValue.ToString().Trim() : "";
            if (col == "Code")
            {
                int c;
                if (!int.TryParse(text, out c) || c <= 0) { dgv.Rows[e.RowIndex].ErrorText = "Code must be a positive integer."; e.Cancel = true; return; }
                dgv.Rows[e.RowIndex].ErrorText = "";
            }
            else if (col == "Name")
            {
                if (text.Length > 50) { dgv.Rows[e.RowIndex].ErrorText = "Name max 50 characters."; e.Cancel = true; return; }
                dgv.Rows[e.RowIndex].ErrorText = "";
            }
            else if (col == "Type")
            {
                if (text.Length > 1) { dgv.Rows[e.RowIndex].ErrorText = "Type is a single character code."; e.Cancel = true; return; }
                dgv.Rows[e.RowIndex].ErrorText = "";
            }
            else if (col == "Main")
            {
                if (text.Length > 6) { dgv.Rows[e.RowIndex].ErrorText = "Main max 6 characters."; e.Cancel = true; return; }
                dgv.Rows[e.RowIndex].ErrorText = "";
            }
            else if (col == "SCode")
            {
                if (!string.IsNullOrEmpty(text))
                {
                    int s;
                    if (!int.TryParse(text, out s)) { dgv.Rows[e.RowIndex].ErrorText = "S-Code must be a number."; e.Cancel = true; return; }
                }
                dgv.Rows[e.RowIndex].ErrorText = "";
            }
            else if (col == "Balance")
            {
                if (!string.IsNullOrEmpty(text))
                {
                    decimal d;
                    if (!decimal.TryParse(text.Replace(",", ""), out d)) { dgv.Rows[e.RowIndex].ErrorText = "Balance must be numeric."; e.Cancel = true; return; }
                }
                dgv.Rows[e.RowIndex].ErrorText = "";
            }
        }

        private void Dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_loading || e.RowIndex < 0 || e.RowIndex >= _rows.Count) return;
            CoaRow row = _rows[e.RowIndex];
            if (!row.IsNew) { row.IsDirty = true; row.Status = "Edited"; }
            else row.Status = "New";
            dgv.Rows[e.RowIndex].ErrorText = "";
            UpdateStatus(null);
        }

        private void Dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_loading || e.RowIndex < 0 || e.RowIndex >= _rows.Count) return;
            CoaRow row = _rows[e.RowIndex];
            if (!row.IsNew) { row.IsDirty = true; if (row.Status != "New") row.Status = "Edited"; }
            UpdateStatus(null);
        }

        private void Dgv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && dgv.IsCurrentCellInEditMode)
            {
                e.Handled = true; e.SuppressKeyPress = true; dgv.EndEdit();
                int col = dgv.CurrentCell != null ? dgv.CurrentCell.ColumnIndex : 0;
                int row = dgv.CurrentCell != null ? dgv.CurrentCell.RowIndex : 0;
                for (int c = col + 1; c < dgv.Columns.Count; c++)
                {
                    if (!dgv.Columns[c].ReadOnly && dgv.Columns[c].Visible)
                    { dgv.CurrentCell = dgv.Rows[row].Cells[c]; return; }
                }
            }
        }

        private void SaveAll()
        {
            if (dgv.IsCurrentCellInEditMode) dgv.EndEdit();
            var pending = new List<CoaRow>();
            foreach (CoaRow r in _rows)
                if (r.IsNew || r.IsDirty) pending.Add(r);
            if (pending.Count == 0)
            {
                MessageBox.Show("No pending changes to save.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var codesInBatch = new HashSet<int>();
            foreach (CoaRow r in pending)
            {
                if (r.Code <= 0) { MessageBox.Show("Each account needs a positive Code.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (string.IsNullOrWhiteSpace(r.Name)) { MessageBox.Show("Account name is required (code " + r.Code + ").", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (r.Name.Trim().Length > 50) { MessageBox.Show("Account name max 50 characters (code " + r.Code + ").", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!string.IsNullOrEmpty(r.Type) && r.Type.Trim().Length > 1) { MessageBox.Show("Type must be a single character (code " + r.Code + ").", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!string.IsNullOrEmpty(r.Main) && r.Main.Length > 6) { MessageBox.Show("Main max 6 characters (code " + r.Code + ").", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                if (!codesInBatch.Add(r.Code)) { MessageBox.Show("Duplicate code in pending rows: " + r.Code, "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            }
            int ok = 0;
            var errors = new List<string>();
            foreach (CoaRow r in pending)
            {
                if (r.IsNew)
                {
                    try
                    {
                        if (_svc.Get(r.Code) != null) { errors.Add("Code " + r.Code + " already exists."); continue; }
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine("ChartOfAccountsForm.Save unique: " + ex.Message);
                        errors.Add("Code " + r.Code + ": could not verify uniqueness."); continue;
                    }
                }
                Account a = r.ToAccount();
                string error;
                if (!_svc.Save(a, out error)) { errors.Add("Code " + r.Code + ": " + (error ?? "save failed")); continue; }
                r.IsNew = false; r.IsDirty = false; r.OriginalCode = r.Code; r.Status = "";
                try { Account saved = _svc.Get(r.Code); if (saved != null) r.Balance = saved.Balance; } catch { }
                ok++;
            }
            UpdateStatus("Saved " + ok + "/" + pending.Count);
            if (errors.Count > 0)
                MessageBox.Show("Saved " + ok + " of " + pending.Count + ".\n\n" + string.Join("\n", errors.Take(8).ToArray()),
                    "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show("Saved " + ok + " account(s).", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information);
            dgv.Refresh();
        }

        private void DeactivateCurrent()
        {
            if (dgv.IsCurrentCellInEditMode) dgv.EndEdit();
            if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0 || dgv.CurrentRow.Index >= _rows.Count)
            {
                MessageBox.Show("Select an account row first.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            CoaRow row = _rows[dgv.CurrentRow.Index];
            if (row.IsNew) { _rows.RemoveAt(dgv.CurrentRow.Index); UpdateStatus("Removed unsaved row"); return; }
            if (row.Sys)
            {
                var r0 = MessageBox.Show("This row is marked as a system account. Deactivate anyway?",
                    "Chart of Accounts", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r0 != DialogResult.Yes) return;
            }
            int code = row.Code;
            try
            {
                bool used = _svc.IsReferenced(code);
                string msg = used
                    ? "Account " + code + " is used in transactions and cannot be deleted.\n\nDeactivate (StopTrans = Y)?"
                    : "Deactivate account " + code + " (StopTrans = Y)?";
                var r = MessageBox.Show(msg, "Chart of Accounts", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
                string error;
                if (!_svc.Deactivate(code, out error))
                {
                    MessageBox.Show(error ?? "Deactivate failed.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                row.Active = false; row.IsDirty = false; row.Status = "";
                UpdateStatus("Deactivated " + code);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ChartOfAccountsForm.Deactivate: " + ex.Message);
                MessageBox.Show("Could not deactivate account.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ChartOfAccountsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { AddNewRow(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { SaveAll(); e.Handled = true; }
            else if (e.KeyCode == Keys.F3) { txtSearch.Focus(); txtSearch.SelectAll(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape)
            {
                if (dgv.IsCurrentCellInEditMode) { dgv.CancelEdit(); e.Handled = true; }
                else if (txtSearch.Focused) { dgv.Focus(); e.Handled = true; }
                else { Close(); e.Handled = true; }
            }
        }

        private sealed class CoaRow
        {
            public bool IsNew { get; set; }
            public bool IsDirty { get; set; }
            public int OriginalCode { get; set; }
            public int Code { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Main { get; set; }
            public int SCode { get; set; }
            public decimal Balance { get; set; }
            public bool Active { get; set; }
            public bool Sys { get; set; }
            public string Status { get; set; }

            public static CoaRow FromAccount(Account a)
            {
                string type = a.Partytype != null ? a.Partytype.Trim() : "";
                if (type.Length > 1) type = type.Substring(0, 1);
                return new CoaRow {
                    IsNew = false, IsDirty = false, OriginalCode = a.acno, Code = a.acno,
                    Name = !string.IsNullOrEmpty(a.NAME) ? a.NAME : (a.dsc ?? ""),
                    Type = type, Main = a.Main ?? "", SCode = a.Scode, Balance = a.Balance,
                    Active = !string.Equals(a.StopTrans, "Y", StringComparison.OrdinalIgnoreCase),
                    Sys = !string.IsNullOrWhiteSpace(a.SysAc) && a.SysAc.Trim() != "",
                    Status = ""
                };
            }

            public Account ToAccount()
            {
                string type = Type != null ? Type.Trim() : "";
                if (type.Length > 1) type = type.Substring(0, 1);
                if (string.IsNullOrEmpty(type)) type = "C";
                return new Account {
                    acno = Code,
                    NAME = Name != null ? Name.Trim() : "",
                    dsc = Name != null ? Name.Trim() : "",
                    Partytype = type,
                    Main = string.IsNullOrWhiteSpace(Main) ? null : Main.Trim(),
                    Scode = SCode,
                    Balance = Balance,
                    StopTrans = Active ? "N" : "Y",
                    SysAc = Sys ? "Y" : " "
                };
            }
        }
    }
}
