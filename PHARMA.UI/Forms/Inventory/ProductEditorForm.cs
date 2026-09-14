using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;
using PHARMA.UI.Helpers;

namespace PHARMA.UI.Forms.Inventory
{
    public class ProductEditorForm : Form
    {
        private readonly ProductService _svc = new ProductService();
        private bool _isNew;
        private string _editCode;

        private TextBox txtCode, txtName, txtPack, txtUnit, txtTp, txtRp, txtPurRate, txtBarcode, txtStock;
        private CheckBox chkActive;
        private Label lblError, lblStockCaption;
        private Button btnSave, btnCancel;
        private bool _dirty;
        private bool _suppressLookup;

        public string SavedCode { get; private set; }

        public static ProductEditorForm ForNew()
        {
            return new ProductEditorForm(true, null);
        }

        public static ProductEditorForm ForEdit(Product existing)
        {
            if (existing == null) throw new ArgumentNullException("existing");
            return new ProductEditorForm(false, existing);
        }

        private ProductEditorForm(bool isNew, Product existing)
        {
            _isNew = isNew;
            _editCode = existing != null ? existing.pcode : null;
            Text = isNew ? "New Product" : "Edit Product";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(440, 360);
            KeyPreview = true;
            BackColor = Color.FromArgb(250, 248, 240);
            Font = new Font("Microsoft Sans Serif", 9F);
            BuildUI();
            if (isNew) LoadNewDefaults();
            else LoadExisting(existing);
            KeyDown += ProductEditorForm_KeyDown;
            FormClosing += ProductEditorForm_FormClosing;
            Shown += ProductEditorForm_Shown;
        }

        private void ProductEditorForm_Shown(object sender, EventArgs e)
        {
            if (_isNew && txtCode != null && txtCode.CanFocus)
            {
                txtCode.Focus();
                txtCode.SelectAll();
            }
            else
                FocusName();
        }

        private void FocusName()
        {
            if (txtName != null && !txtName.IsDisposed && txtName.CanFocus)
            {
                txtName.Focus();
                txtName.SelectAll();
            }
        }

        private void BuildUI()
        {
            int y = 14;
            Controls.Add(L("Code", 16, y + 2));
            txtCode = Tb(110, y, 120);
            txtCode.Leave += TxtCode_Leave;
            txtCode.KeyDown += TxtCode_KeyDown;
            Controls.Add(txtCode);

            y += 28;
            Controls.Add(L("Name", 16, y + 2));
            txtName = Tb(110, y, 300);
            Controls.Add(txtName);

            y += 28;
            Controls.Add(L("Pack", 16, y + 2));
            txtPack = Tb(110, y, 100);
            Controls.Add(txtPack);
            Controls.Add(L("Unit", 230, y + 2));
            txtUnit = Tb(270, y, 60);
            Controls.Add(txtUnit);

            y += 28;
            Controls.Add(L("TP", 16, y + 2));
            txtTp = Tb(110, y, 90);
            Controls.Add(txtTp);
            Controls.Add(L("RP", 220, y + 2));
            txtRp = Tb(250, y, 90);
            Controls.Add(txtRp);

            y += 28;
            Controls.Add(L("Pur Rate", 16, y + 2));
            txtPurRate = Tb(110, y, 90);
            Controls.Add(txtPurRate);

            y += 28;
            Controls.Add(L("Barcode", 16, y + 2));
            txtBarcode = Tb(110, y, 200);
            Controls.Add(txtBarcode);

            y += 28;
            lblStockCaption = L(_isNew ? "Opening Stock" : "Stock", 16, y + 2);
            Controls.Add(lblStockCaption);
            txtStock = Tb(110, y, 90);
            Controls.Add(txtStock);

            y += 30;
            chkActive = new CheckBox { Text = "Active", Location = new Point(110, y), AutoSize = true, Checked = true };
            Controls.Add(chkActive);

            y += 32;
            lblError = new Label { Location = new Point(16, y), Size = new Size(400, 36), ForeColor = Color.DarkRed, Text = "" };
            Controls.Add(lblError);

            btnSave = new Button { Text = "Save (F5)", Location = new Point(230, 320), Size = new Size(90, 28) };
            btnCancel = new Button { Text = "Cancel (Esc)", Location = new Point(330, 320), Size = new Size(90, 28) };
            btnSave.Click += (s, e) => SaveAndCloseIfOk();
            btnCancel.Click += (s, e) => TryCancel();
            Controls.Add(btnSave);
            Controls.Add(btnCancel);
            AcceptButton = null;
            CancelButton = btnCancel;

            EventHandler mark = (s, e) => { _dirty = true; ClearError(); };
            foreach (var t in new[] { txtCode, txtName, txtPack, txtUnit, txtTp, txtRp, txtPurRate, txtBarcode, txtStock })
                t.TextChanged += mark;
            chkActive.CheckedChanged += mark;

            foreach (var t in new[] { txtName, txtPack, txtUnit, txtTp, txtRp, txtPurRate, txtBarcode, txtStock })
                t.KeyDown += Field_KeyDown;
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
            _suppressLookup = true;
            txtCode.ReadOnly = false;
            txtCode.BackColor = Color.White;
            txtCode.Clear();
            txtName.Clear();
            txtPack.Clear();
            txtUnit.Text = "1";
            txtTp.Clear();
            txtRp.Clear();
            txtPurRate.Clear();
            txtBarcode.Clear();
            if (lblStockCaption != null) lblStockCaption.Text = "Opening Stock";
            txtStock.ReadOnly = false;
            txtStock.BackColor = Color.White;
            txtStock.Text = "0";
            chkActive.Checked = true;
            _dirty = false;
            _suppressLookup = false;
        }

        private void LoadExisting(Product p)
        {
            if (p == null) return;
            _suppressLookup = true;
            txtCode.Text = p.pcode ?? "";
            txtCode.ReadOnly = true;
            txtCode.BackColor = Color.WhiteSmoke;
            txtName.Text = p.name1 ?? "";
            txtPack.Text = p.pack ?? "";
            txtUnit.Text = p.unit > 0 ? p.unit.ToString() : "1";
            txtTp.Text = p.tp.ToString("0.####");
            txtRp.Text = p.rp.ToString("0.####");
            txtPurRate.Text = p.Pur_Rate.ToString("0.####");
            txtBarcode.Text = p.BarCode1 ?? "";
            if (lblStockCaption != null) lblStockCaption.Text = "Stock";
            txtStock.Text = p.balance.ToString();
            txtStock.ReadOnly = true;
            txtStock.BackColor = Color.WhiteSmoke;
            chkActive.Checked = string.IsNullOrEmpty(p.Active) || string.Equals(p.Active, "Y", StringComparison.OrdinalIgnoreCase);
            _dirty = false;
            _suppressLookup = false;
        }

        private void SwitchToEditMode(Product existing)
        {
            _isNew = false;
            _editCode = existing.pcode;
            Text = "Edit Product";
            LoadExisting(existing);
            ClearError();
            FocusName();
        }

        private void TryLookupExistingCode()
        {
            if (_suppressLookup || !_isNew || txtCode.ReadOnly) return;
            string code = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(code)) return;
            try
            {
                Product existing = _svc.Get(code);
                if (existing == null) return;
                SwitchToEditMode(existing);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("ProductEditorForm.TryLookupExistingCode: " + ex.Message);
                ShowError("Could not look up product code. Check the connection.", txtCode);
            }
        }

        private void TxtCode_Leave(object sender, EventArgs e) { TryLookupExistingCode(); }

        private void TxtCode_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                TryLookupExistingCode();
                if (_isNew) SelectNextControl(txtCode, true, true, true, true);
            }
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

        private void ProductEditorForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5) { SaveAndCloseIfOk(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.Enter) { SaveAndCloseIfOk(); e.Handled = true; }
            else if (e.KeyCode == Keys.Escape) { TryCancel(); e.Handled = true; }
        }

        private void ProductEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.OK) return;
            if (!_dirty) return;
            UiStyle.UnsavedChoice choice = UiStyle.ConfirmUnsavedChanges(this, Text);
            if (choice == UiStyle.UnsavedChoice.Cancel) { e.Cancel = true; return; }
            if (choice == UiStyle.UnsavedChoice.DontSave) { _dirty = false; return; }
            if (!PersistChanges()) e.Cancel = true;
            else { DialogResult = DialogResult.OK; _dirty = false; }
        }

        private void TryCancel() { Close(); }

        private void SaveAndCloseIfOk()
        {
            if (!PersistChanges()) return;
            _dirty = false;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ClearError() { lblError.Text = ""; }

        private void ShowError(string msg, Control focus)
        {
            lblError.Text = msg;
            if (focus != null) focus.Focus();
        }

        private bool PersistChanges()
        {
            ClearError();
            if (_isNew && !txtCode.ReadOnly) TryLookupExistingCode();

            string code = txtCode.Text.Trim();
            if (string.IsNullOrEmpty(code))
            { ShowError("Product code is required.", txtCode); return false; }
            if (code.Length > 50)
            { ShowError("Product code is too long.", txtCode); return false; }

            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            { ShowError("Product name is required.", txtName); return false; }
            if (name.Length > 100)
            { ShowError("Product name is too long.", txtName); return false; }

            int unit = 1;
            if (!string.IsNullOrWhiteSpace(txtUnit.Text))
            {
                if (!int.TryParse(txtUnit.Text.Trim(), out unit) || unit <= 0)
                { ShowError("Unit must be a whole number greater than zero.", txtUnit); return false; }
            }

            decimal tp = 0, rp = 0, pur = 0;
            if (!string.IsNullOrWhiteSpace(txtTp.Text) && !decimal.TryParse(txtTp.Text.Trim(), out tp))
            { ShowError("Trade price (TP) must be numeric.", txtTp); return false; }
            if (!string.IsNullOrWhiteSpace(txtRp.Text) && !decimal.TryParse(txtRp.Text.Trim(), out rp))
            { ShowError("Retail price (RP) must be numeric.", txtRp); return false; }
            if (!string.IsNullOrWhiteSpace(txtPurRate.Text) && !decimal.TryParse(txtPurRate.Text.Trim(), out pur))
            { ShowError("Purchase rate must be numeric.", txtPurRate); return false; }
            if (tp < 0 || rp < 0 || pur < 0)
            { ShowError("Prices cannot be negative.", null); return false; }

            int stock = 0;
            if (_isNew && !string.IsNullOrWhiteSpace(txtStock.Text))
            {
                if (!int.TryParse(txtStock.Text.Trim(), out stock) || stock < 0)
                { ShowError("Opening stock must be a non-negative whole number.", txtStock); return false; }
            }

            if (_isNew)
            {
                try
                {
                    Product existing = _svc.Get(code);
                    if (existing != null)
                    {
                        SwitchToEditMode(existing);
                        ShowError("Product already exists \u2014 switched to Edit. Review and press F5 to save.", txtName);
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("ProductEditorForm.Persist unique: " + ex.Message);
                    ShowError("Could not verify product code uniqueness.", txtCode);
                    return false;
                }
            }

            var p = new Product
            {
                pcode = code,
                name1 = name,
                pack = string.IsNullOrWhiteSpace(txtPack.Text) ? null : txtPack.Text.Trim(),
                unit = unit,
                tp = tp,
                rp = rp,
                Pur_Rate = pur,
                BarCode1 = string.IsNullOrWhiteSpace(txtBarcode.Text) ? null : txtBarcode.Text.Trim(),
                Active = chkActive.Checked ? "Y" : "N",
                balance = stock
            };

            string error;
            if (!_svc.Save(p, out error))
            {
                ShowError(string.IsNullOrEmpty(error) ? "Save failed." : error, null);
                return false;
            }

            SavedCode = code;
            _dirty = false;
            return true;
        }
    }
}
