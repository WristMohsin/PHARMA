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
        private TextBox txtSearch;
        private DataGridView dgv;
        private Label lblStatus;
        private Button btnNew, btnDeactivate, btnSearch, btnRefresh, btnClose;

        public ChartOfAccountsForm()
        {
            Text = "Chart of Accounts";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Microsoft Sans Serif", 9F);
            BuildUI();
            ReloadFromDb("");
            KeyDown += ChartOfAccountsForm_KeyDown;
        }

        private void BuildUI()
        {
            var tool = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.FromArgb(245, 240, 225) };
            txtSearch = new TextBox { Location = new Point(6, 8), Size = new Size(200, 22), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.KeyDown += TxtSearch_KeyDown;
            btnSearch = MkBtn("Search", 212, 6, 64);
            btnRefresh = MkBtn("Refresh", 280, 6, 64);
            btnNew = MkBtn("New (F2)", 360, 6, 72);
            btnDeactivate = MkBtn("Deactivate", 436, 6, 80);
            btnClose = MkBtn("Close (Esc)", 520, 6, 80);
            btnSearch.Click += (s, e) => ReloadFromDb(txtSearch.Text.Trim());
            btnRefresh.Click += (s, e) => { txtSearch.Clear(); ReloadFromDb(""); };
            btnNew.Click += (s, e) => OpenNew();
            btnDeactivate.Click += (s, e) => DeactivateSelected();
            btnClose.Click += (s, e) => Close();
            tool.Controls.AddRange(new Control[] { txtSearch, btnSearch, btnRefresh, btnNew, btnDeactivate, btnClose });

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 28, BackColor = Color.FromArgb(245, 240, 225) };
            lblStatus = new Label {
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(6, 0, 0, 0),
                ForeColor = Color.DimGray,
                Text = "F2 New  Enter/Double-click Edit  F3 Search  Esc Close  |  Grid is read-only"
            };
            footer.Controls.Add(lblStatus);

            dgv = new DataGridView {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                ReadOnly = true, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false, BackgroundColor = Color.White, BorderStyle = BorderStyle.Fixed3D,
                AutoGenerateColumns = false, EditMode = DataGridViewEditMode.EditProgrammatically
            };
            dgv.RowTemplate.Height = 22;
            dgv.ColumnHeadersHeight = 24;
            UiStyle.StyleGrid(dgv);
            BuildColumns();
            dgv.CellDoubleClick += Dgv_CellDoubleClick;
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
            dgv.Columns.Add(Col("Code", "Code", 70));
            dgv.Columns.Add(Col("Name", "Account Name", 220));
            dgv.Columns.Add(Col("Type", "Type", 50));
            dgv.Columns.Add(Col("Main", "Main", 60));
            dgv.Columns.Add(Col("SCode", "S-Code", 60));
            var bal = Col("Balance", "Balance", 90);
            bal.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            bal.DefaultCellStyle.Format = "N2";
            dgv.Columns.Add(bal);
            dgv.Columns.Add(Col("Active", "Active", 55));
            dgv.Columns.Add(Col("Sys", "Sys", 40));
            dgv.Columns.Add(Col("Status", "Status", 70));
        }

        private static DataGridViewTextBoxColumn Col(string name, string header, int width)
        {
            return new DataGridViewTextBoxColumn {
                Name = name, DataPropertyName = name, HeaderText = header, Width = width, ReadOnly = true,
                SortMode = DataGridViewColumnSortMode.NotSortable
            };
        }

        private void ReloadFromDb(string term)
        {
            try
            {
                var list = _svc.Search(term);
                dgv.DataSource = null;
                dgv.DataSource = list.Select(a => new {
                    Code = a.acno,
                    Name = !string.IsNullOrEmpty(a.NAME) ? a.NAME : a.dsc,
                    Type = FormatType(a.Partytype),
                    Main = a.Main,
                    SCode = a.Scode,
                    Balance = a.Balance,
                    Active = string.Equals(a.StopTrans, "Y", StringComparison.OrdinalIgnoreCase) ? "No" : "Yes",
                    Sys = !string.IsNullOrWhiteSpace(a.SysAc) && a.SysAc.Trim() != "" ? "Y" : "",
                    Status = ""
                }).ToList();
                lblStatus.Text = "Rows: " + list.Count +
                    "  |  F2 New  Enter/Double-click Edit  F3 Search  Esc Close  |  Type = Partytype as stored";
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ChartOfAccountsForm.ReloadFromDb: " + ex.Message);
                MessageBox.Show("Could not load accounts. Check the database connection.",
                    "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static string FormatType(string partytype)
        {
            if (string.IsNullOrEmpty(partytype)) return "";
            string t = partytype.Trim();
            if (t.Length > 1) t = t.Substring(0, 1);
            return t.ToUpperInvariant();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { ReloadFromDb(txtSearch.Text.Trim()); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) { dgv.Focus(); e.Handled = true; }
        }

        private void OpenNew()
        {
            using (var f = AccountEditorForm.ForNew())
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    ReloadFromDb(txtSearch.Text.Trim());
                    SelectCode(f.SavedCode);
                }
            }
        }

        private void OpenEditSelected()
        {
            int code = GetSelectedCode();
            if (code <= 0)
            {
                MessageBox.Show("Select an account row first.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Account a;
            try { a = _svc.Get(code); }
            catch (Exception ex)
            {
                Trace.WriteLine("ChartOfAccountsForm.OpenEdit: " + ex.Message);
                MessageBox.Show("Could not load account.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (a == null)
            {
                MessageBox.Show("Account not found: " + code, "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var f = AccountEditorForm.ForEdit(a))
            {
                if (f.ShowDialog(this) == DialogResult.OK)
                {
                    ReloadFromDb(txtSearch.Text.Trim());
                    SelectCode(f.SavedCode);
                }
            }
        }

        private int GetSelectedCode()
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0) return 0;
            object v = dgv.CurrentRow.Cells["Code"].Value;
            if (v == null) return 0;
            int code;
            return int.TryParse(v.ToString(), out code) ? code : 0;
        }

        private void SelectCode(int code)
        {
            if (code <= 0 || dgv.Rows.Count == 0) return;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells["Code"].Value != null && row.Cells["Code"].Value.ToString() == code.ToString())
                {
                    row.Selected = true;
                    dgv.CurrentCell = row.Cells["Code"];
                    try { dgv.FirstDisplayedScrollingRowIndex = row.Index; } catch { }
                    break;
                }
            }
        }

        private void Dgv_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) OpenEditSelected();
        }

        private void Dgv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                e.SuppressKeyPress = true;
                OpenEditSelected();
            }
        }

        private void DeactivateSelected()
        {
            int code = GetSelectedCode();
            if (code <= 0)
            {
                MessageBox.Show("Select an account row first.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            bool isSys = false;
            if (dgv.CurrentRow != null && dgv.CurrentRow.Cells["Sys"].Value != null)
                isSys = dgv.CurrentRow.Cells["Sys"].Value.ToString() == "Y";
            if (isSys)
            {
                var r0 = MessageBox.Show("This row is marked as a system account. Deactivate anyway?",
                    "Chart of Accounts", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r0 != DialogResult.Yes) return;
            }
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
                ReloadFromDb(txtSearch.Text.Trim());
                SelectCode(code);
                lblStatus.Text = "Deactivated " + code + "  |  F2 New  Enter Edit  F3 Search  Esc Close";
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ChartOfAccountsForm.Deactivate: " + ex.Message);
                MessageBox.Show("Could not deactivate account.", "Chart of Accounts", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ChartOfAccountsForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { OpenNew(); e.Handled = true; }
            else if (e.KeyCode == Keys.F3) { txtSearch.Focus(); txtSearch.SelectAll(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape)
            {
                if (txtSearch.Focused) { dgv.Focus(); e.Handled = true; }
                else { Close(); e.Handled = true; }
            }
        }
    }
}
