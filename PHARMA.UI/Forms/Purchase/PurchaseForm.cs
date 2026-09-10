using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;
using PHARMA.UI.Helpers;
using PHARMA.UI.Forms.Sale;

namespace PHARMA.UI.Forms.Purchase
{
    public class PurchaseForm : Form
    {
        private readonly PurchaseService _svc = new PurchaseService();
        private readonly AccountService _accService = new AccountService();

        private BindingList<PurLineView> _views = new BindingList<PurLineView>();
        private int _invNo;
        private bool _dirty;
        private bool _suppressGridEvents;

        private TextBox txtInvNo, txtParty, txtSearch, txtDocRef;
        private DataGridView dgv;
        private Label lblDate, lblPartyName, lblGross, lblDisc, lblNet, lblHint;
        private Button btnNew, btnSearch, btnSave, btnClose;

        public PurchaseForm()
        {
            Text = "Purchase";
            KeyPreview = true;
            WindowState = FormWindowState.Maximized;
            BackColor = Color.FromArgb(255, 250, 240);
            Font = new Font("Microsoft Sans Serif", 8.25F);
            FormClosing += PurchaseForm_FormClosing;
            BuildUI();
            NewDoc(true);
            KeyDown += PurchaseForm_KeyDown;
        }

        private void PurchaseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing || e.CloseReason == CloseReason.ApplicationExitCall)
            {
                if (_dirty || _views.Count > 0)
                {
                    if (!UiStyle.ConfirmClose(this, "Purchase"))
                        e.Cancel = true;
                }
            }
        }

        private void BuildUI()
        {
            var header = new Panel { Dock = DockStyle.Top, Height = 78, BackColor = Color.FromArgb(255, 248, 230), Padding = new Padding(4) };
            int y1 = 6;
            header.Controls.Add(new Label { Text = "Inv #", Location = new Point(6, y1 + 2), AutoSize = true });
            txtInvNo = new TextBox { Location = new Point(40, y1), Size = new Size(70, 20), ReadOnly = true, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            header.Controls.Add(txtInvNo);
            header.Controls.Add(new Label { Text = "Date", Location = new Point(118, y1 + 2), AutoSize = true });
            lblDate = new Label { Location = new Point(150, y1 + 2), AutoSize = true, Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold) };
            header.Controls.Add(lblDate);
            header.Controls.Add(new Label { Text = "Doc No", Location = new Point(250, y1 + 2), AutoSize = true });
            txtDocRef = new TextBox { Location = new Point(300, y1), Size = new Size(90, 20), BorderStyle = BorderStyle.FixedSingle };
            header.Controls.Add(txtDocRef);
            header.Controls.Add(new Label { Text = "Party", Location = new Point(400, y1 + 2), AutoSize = true });
            txtParty = new TextBox { Location = new Point(435, y1), Size = new Size(60, 20), BorderStyle = BorderStyle.FixedSingle };
            txtParty.Leave += TxtParty_Leave;
            txtParty.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { TxtParty_Leave(s, EventArgs.Empty); txtSearch.Focus(); e.SuppressKeyPress = true; } };
            header.Controls.Add(txtParty);
            lblPartyName = new Label { Location = new Point(500, y1 + 2), Size = new Size(420, 16), ForeColor = Color.DarkBlue };
            header.Controls.Add(lblPartyName);
            int y2 = 36;
            header.Controls.Add(new Label { Text = "Product", Location = new Point(6, y2 + 2), AutoSize = true });
            txtSearch = new TextBox { Location = new Point(55, y2), Size = new Size(320, 22), Font = new Font("Consolas", 9.5F), BorderStyle = BorderStyle.FixedSingle };
            txtSearch.KeyDown += TxtSearch_KeyDown;
            header.Controls.Add(txtSearch);
            btnSearch = new Button { Text = "F4 Search", Location = new Point(380, y2 - 1), Size = new Size(80, 24), FlatStyle = FlatStyle.System };
            btnSearch.Click += (s, e) => OpenSearchAndPick(txtSearch.Text.Trim());
            header.Controls.Add(btnSearch);
            lblHint = new Label { Text = "Enter/F4 product -> Qty-P/Qty-L in grid -> Enter  |  F2 New  F5 Save  Del  Esc", Location = new Point(470, y2 + 4), AutoSize = true, ForeColor = Color.DimGray };
            header.Controls.Add(lblHint);

            var footer = new Panel { Dock = DockStyle.Bottom, Height = 48, BackColor = Color.FromArgb(245, 240, 220) };
            btnNew = new Button { Text = "New (F2)", Location = new Point(8, 10), Size = new Size(78, 28) };
            btnSave = new Button { Text = "Save (F5)", Location = new Point(92, 10), Size = new Size(78, 28) };
            btnClose = new Button { Text = "Close (Esc)", Location = new Point(176, 10), Size = new Size(84, 28) };
            btnNew.Click += (s, e) => NewDoc(false);
            btnSave.Click += (s, e) => Save();
            btnClose.Click += (s, e) => Close();
            footer.Controls.AddRange(new Control[] { btnNew, btnSave, btnClose });
            lblGross = new Label { Location = new Point(280, 6), AutoSize = true };
            lblDisc = new Label { Location = new Point(280, 24), AutoSize = true };
            lblNet = new Label { Location = new Point(420, 12), AutoSize = true, Font = new Font("Microsoft Sans Serif", 11F, FontStyle.Bold), ForeColor = Color.FromArgb(0, 90, 50) };
            footer.Controls.Add(lblGross);
            footer.Controls.Add(lblDisc);
            footer.Controls.Add(lblNet);

            dgv = new DataGridView {
                Dock = DockStyle.Fill, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
                RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.CellSelect, MultiSelect = false,
                BackgroundColor = Color.White, BorderStyle = BorderStyle.Fixed3D, Font = new Font("Microsoft Sans Serif", 8.25F),
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(210, 190, 140), ForeColor = Color.Black,
                    Font = new Font("Microsoft Sans Serif", 8.25F, FontStyle.Bold), Alignment = DataGridViewContentAlignment.MiddleCenter },
                EnableHeadersVisualStyles = false, AutoGenerateColumns = false,
                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2, StandardTab = true };
            dgv.RowTemplate.Height = 20;
            dgv.ColumnHeadersHeight = 22;
            BuildGridColumns();
            dgv.DataSource = _views;
            dgv.CellEndEdit += Dgv_CellEndEdit;
            dgv.CellValidating += Dgv_CellValidating;
            dgv.KeyDown += Dgv_KeyDown;
            dgv.DataError += (s, e) => { e.ThrowException = false; };

            Controls.Add(dgv);
            Controls.Add(footer);
            Controls.Add(header);
        }

        private void BuildGridColumns()
        {
            dgv.Columns.Clear();
            dgv.Columns.Add(MakeCol("Description", "Description", 180, true));
            dgv.Columns.Add(MakeCol("Batch", "Batch", 70, false));
            dgv.Columns.Add(MakeCol("ExpDt", "Exp Dt", 75, false));
            dgv.Columns.Add(MakeCol("QtyP", "Qty-P", 55, false));
            dgv.Columns.Add(MakeCol("QtyL", "Qty-L", 50, false));
            dgv.Columns.Add(MakeCol("Rate", "Rate", 70, false));
            dgv.Columns.Add(MakeCol("DiscPct", "Disc%", 50, false));
            dgv.Columns.Add(MakeCol("Free", "Free", 45, false));
            dgv.Columns.Add(MakeCol("STax", "S.Tax", 55, true));
            dgv.Columns.Add(MakeCol("NetAmount", "Net Amount", 85, true));
            dgv.Columns.Add(MakeCol("ATax", "A.Tax", 50, true));
            dgv.Columns.Add(MakeCol("Code", "Code", 70, true));
            dgv.Columns.Add(MakeCol("UnitsPerPack", "U/P", 40, true));
            SetNum("QtyP"); SetNum("QtyL"); SetNum("Rate", "N2"); SetNum("DiscPct", "N2");
            SetNum("NetAmount", "N2"); SetNum("STax", "N2"); SetNum("ATax", "N2"); SetNum("Free"); SetNum("UnitsPerPack");
        }

        private void SetNum(string name, string format = null)
        {
            if (!dgv.Columns.Contains(name)) return;
            dgv.Columns[name].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            if (format != null) dgv.Columns[name].DefaultCellStyle.Format = format;
        }

        private static DataGridViewTextBoxColumn MakeCol(string prop, string header, int width, bool readOnly)
        {
            return new DataGridViewTextBoxColumn {
                DataPropertyName = prop, HeaderText = header, Name = prop, Width = width,
                ReadOnly = readOnly, SortMode = DataGridViewColumnSortMode.NotSortable };
        }

        private void TxtParty_Leave(object sender, EventArgs e)
        {
            int code = 0;
            int.TryParse(txtParty.Text.Trim(), out code);
            if (code <= 0) { lblPartyName.Text = "Cash / Default"; lblPartyName.ForeColor = Color.DarkBlue; return; }
            try {
                var a = _accService.Get(code);
                if (a != null) {
                    string name = a.NAME != null ? a.NAME : (a.dsc != null ? a.dsc : code.ToString());
                    lblPartyName.Text = name + "   Bal: " + a.Balance.ToString("N2");
                    lblPartyName.ForeColor = Color.DarkBlue;
                } else { lblPartyName.Text = "(supplier not found)"; lblPartyName.ForeColor = Color.DarkRed; }
            } catch (Exception ex) {
                Trace.WriteLine("PurchaseForm: supplier lookup failed: " + ex.Message);
                lblPartyName.Text = "(lookup error)"; lblPartyName.ForeColor = Color.DarkRed;
            }
        }

        private void NewDoc(bool force)
        {
            if (!force && (_dirty || _views.Count > 0)) {
                var r = MessageBox.Show("Clear current purchase and start new?", "New Purchase", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (r != DialogResult.Yes) return;
            }
            _suppressGridEvents = true; _views.Clear(); _suppressGridEvents = false;
            _dirty = false;
            _invNo = _svc.NextInvNo();
            txtInvNo.Text = _invNo.ToString();
            lblDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            txtParty.Text = "0"; txtDocRef.Clear();
            lblPartyName.Text = "Cash / Default"; lblPartyName.ForeColor = Color.DarkBlue;
            txtSearch.Clear(); UpdateTotals(); txtSearch.Focus();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) {
                e.Handled = true; e.SuppressKeyPress = true;
                string q = txtSearch.Text.Trim();
                if (string.IsNullOrEmpty(q)) { OpenSearchAndPick(""); return; }
                try {
                    var p = _svc.FindProduct(q);
                    if (p != null) AddOrFocusProduct(p); else OpenSearchAndPick(q);
                } catch (Exception ex) {
                    Trace.WriteLine("PurchaseForm: product lookup failed: " + ex.Message);
                    MessageBox.Show("Product lookup failed.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            } else if (e.KeyCode == Keys.F4) { OpenSearchAndPick(txtSearch.Text.Trim()); e.Handled = true; }
        }

        private void OpenSearchAndPick(string filter)
        {
            using (var dlg = new ProductSearchPopup(filter))
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.SelectedProduct != null)
                    AddOrFocusProduct(dlg.SelectedProduct);
        }

        private void AddOrFocusProduct(Product p)
        {
            if (p == null || string.IsNullOrEmpty(p.pcode)) return;
            string name = !string.IsNullOrEmpty(p.name1) ? p.name1 : (p.Desc1 ?? p.pcode);
            decimal rate = p.Pur_Rate > 0 ? p.Pur_Rate : p.tp;
            int unitsPerPack = p.unit > 0 ? p.unit : 1;
            PurLineView existing = null;
            foreach (PurLineView v in _views)
                if (string.Equals(v.Code, p.pcode, StringComparison.OrdinalIgnoreCase)) { existing = v; break; }
            int rowIndex;
            if (existing != null) rowIndex = _views.IndexOf(existing);
            else {
                var line = new PurLineView {
                    Description = name, Code = p.pcode, Batch = "", ExpDt = "",
                    QtyP = 1, QtyL = 0, Rate = rate, DiscPct = 0, Free = 0, STax = 0, ATax = 0, UnitsPerPack = unitsPerPack };
                line.RecalcNet(); _views.Add(line); rowIndex = _views.Count - 1;
            }
            _dirty = true; txtSearch.Clear(); UpdateTotals();
            if (rowIndex >= 0 && rowIndex < dgv.Rows.Count) {
                dgv.Focus(); dgv.CurrentCell = dgv.Rows[rowIndex].Cells["QtyP"]; dgv.BeginEdit(true);
            }
        }

        private void Dgv_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (_suppressGridEvents || e.RowIndex < 0) return;
            string col = dgv.Columns[e.ColumnIndex].Name;
            string text = e.FormattedValue != null ? e.FormattedValue.ToString().Trim() : "";
            if (col == "QtyP") { int q; if (!int.TryParse(text, out q) || q < 0) { MessageBox.Show("Qty-P must be >= 0.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning); e.Cancel = true; } }
            else if (col == "QtyL") { int q; if (!int.TryParse(text, out q) || q < 0) { MessageBox.Show("Qty-L must be >= 0.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning); e.Cancel = true; } }
            else if (col == "Rate") { decimal r; if (!decimal.TryParse(text, out r) || r < 0) { MessageBox.Show("Rate cannot be negative.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning); e.Cancel = true; } }
            else if (col == "DiscPct") { decimal d; if (!decimal.TryParse(text, out d) || d < 0) { MessageBox.Show("Disc% cannot be negative.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning); e.Cancel = true; } }
            else if (col == "Free") { int f; if (!int.TryParse(text, out f) || f < 0) { MessageBox.Show("Free must be >= 0.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning); e.Cancel = true; } }
            else if (col == "ExpDt" && !string.IsNullOrEmpty(text)) {
                DateTime dt; if (!TryParseExpiry(text, out dt)) { MessageBox.Show("Invalid expiry. Use dd/MM/yyyy or MM/yyyy.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning); e.Cancel = true; }
            }
        }

        private static bool TryParseExpiry(string text, out DateTime dt)
        {
            dt = DateTime.MinValue;
            if (string.IsNullOrWhiteSpace(text)) return true;
            text = text.Trim();
            string[] formats = new string[] { "dd/MM/yyyy", "d/M/yyyy", "dd-MM-yyyy", "d-M-yyyy", "MM/yyyy", "M/yyyy", "MM-yyyy", "yyyy-MM-dd", "yyyy/MM/dd" };
            if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out dt)) return true;
            return DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out dt);
        }

        private void Dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (_suppressGridEvents || e.RowIndex < 0 || e.RowIndex >= _views.Count) return;
            var line = _views[e.RowIndex];
            line.RecalcNet(); _dirty = true; UpdateTotals(); dgv.InvalidateRow(e.RowIndex);
            string col = dgv.Columns[e.ColumnIndex].Name;
            if (col == "QtyP" || col == "QtyL" || col == "Rate")
                BeginInvoke(new Action(delegate { txtSearch.Focus(); }));
        }

        private void Dgv_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete && !dgv.IsCurrentCellInEditMode) { RemoveSelectedLine(); e.Handled = true; }
            else if (e.KeyCode == Keys.Enter && dgv.IsCurrentCellInEditMode) { e.Handled = true; e.SuppressKeyPress = true; dgv.EndEdit(); }
        }

        private void RemoveSelectedLine()
        {
            if (dgv.CurrentRow == null || dgv.CurrentRow.Index < 0) return;
            int idx = dgv.CurrentRow.Index;
            if (idx >= 0 && idx < _views.Count) { _views.RemoveAt(idx); _dirty = _views.Count > 0; UpdateTotals(); }
        }

        private void UpdateTotals()
        {
            decimal gross = 0, discAmt = 0;
            foreach (PurLineView v in _views) {
                v.RecalcNet();
                decimal lineGross = v.LineGross();
                gross += lineGross;
                discAmt += lineGross * (v.DiscPct / 100m);
            }
            lblGross.Text = "Gross: " + gross.ToString("N2");
            lblDisc.Text = "Disc: " + discAmt.ToString("N2");
            lblNet.Text = "Net: " + (gross - discAmt).ToString("N2");
        }

        private List<pur_det> BuildDetails(int partyCode, DateTime invdt)
        {
            var list = new List<pur_det>();
            int sort = 1;
            foreach (PurLineView v in _views) {
                int upp = v.UnitsPerPack > 0 ? v.UnitsPerPack : 1;
                int stockQty = v.QtyP * upp + v.QtyL;
                if (stockQty <= 0) continue;
                DateTime? exp = null;
                if (!string.IsNullOrWhiteSpace(v.ExpDt)) {
                    DateTime dt; if (TryParseExpiry(v.ExpDt, out dt)) exp = dt;
                }
                list.Add(new pur_det {
                    invno = _invNo, invdt = invdt, code = partyCode, pcode = v.Code,
                    rate = v.Rate, PcRt = v.Rate, qty = stockQty, bonus = v.Free,
                    batchno = string.IsNullOrWhiteSpace(v.Batch) ? null : v.Batch.Trim(),
                    expdt = exp, dip = v.DiscPct, dip2 = 0, stxPerItem = v.STax, type = 1, SortNo = sort++ });
            }
            return list;
        }

        private void Save()
        {
            if (dgv.IsCurrentCellInEditMode) dgv.EndEdit();
            if (_views.Count == 0) { MessageBox.Show("No items to save.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }
            int party = 0; int.TryParse(txtParty.Text.Trim(), out party);
            if (party > 0) {
                try {
                    var a = _accService.Get(party);
                    if (a == null) { MessageBox.Show("Invalid supplier code. Use 0 for Cash/Default.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtParty.Focus(); return; }
                } catch (Exception ex) {
                    Trace.WriteLine("PurchaseForm: supplier validation failed: " + ex.Message);
                    MessageBox.Show("Could not validate supplier.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
                }
            }
            DateTime invdt = DateTime.Now;
            var details = BuildDetails(party, invdt);
            if (details.Count == 0) {
                MessageBox.Show("Each line needs Qty-P and/or Qty-L so total units > 0.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            decimal gross = 0, discAmt = 0;
            foreach (PurLineView v in _views) { decimal lg = v.LineGross(); gross += lg; discAmt += lg * (v.DiscPct / 100m); }
            var header = new purchase {
                invno = _invNo, invdt = invdt, code = party,
                docno = string.IsNullOrWhiteSpace(txtDocRef.Text) ? null : txtDocRef.Text.Trim(),
                docdt = invdt, grsamt = gross, disc = discAmt, xdisc = 0, stax = 0, Freight = 0,
                net = gross - discAmt, type = 1, remarks = null,
                Operator = AuthService.CurrentUser != null ? AuthService.CurrentUser.UserName : "",
                ComputerName = Environment.MachineName, shiftno = 0, WHT = 0 };
            string error;
            if (!_svc.Save(header, details, out error)) {
                MessageBox.Show(error ?? "Save failed.", "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }
            MessageBox.Show("Purchase saved. Inv #: " + _invNo, "Purchase", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _dirty = false; _views.Clear(); NewDoc(true);
        }

        private void PurchaseForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F2) { NewDoc(false); e.Handled = true; }
            else if (e.KeyCode == Keys.F4) { OpenSearchAndPick(txtSearch.Text.Trim()); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { Save(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { Close(); e.Handled = true; }
        }

        private sealed class PurLineView
        {
            public string Description { get; set; }
            public string Batch { get; set; }
            public string ExpDt { get; set; }
            public int QtyP { get; set; }
            public int QtyL { get; set; }
            public decimal Rate { get; set; }
            public decimal DiscPct { get; set; }
            public int Free { get; set; }
            public decimal STax { get; set; }
            public decimal NetAmount { get; set; }
            public decimal ATax { get; set; }
            public string Code { get; set; }
            public int UnitsPerPack { get; set; }

            public decimal LineGross()
            {
                int upp = UnitsPerPack > 0 ? UnitsPerPack : 1;
                decimal packAmt = QtyP * Rate;
                decimal looseAmt = (QtyL > 0 && upp > 0) ? QtyL * (Rate / upp) : 0;
                return packAmt + looseAmt;
            }

            public void RecalcNet()
            {
                decimal g = LineGross();
                NetAmount = Math.Round(g - g * (DiscPct / 100m), 2);
            }
        }
    }
}
