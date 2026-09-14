using System.Drawing;
using System.Windows.Forms;

namespace PHARMA.UI.Helpers
{
    public static class UiStyle
    {
        public static readonly Color Primary = Color.FromArgb(0, 120, 215);
        public static readonly Color PrimaryDark = Color.FromArgb(0, 90, 170);
        public static readonly Color Bg = Color.FromArgb(245, 247, 250);
        public static readonly Color PanelBg = Color.White;
        public static readonly Color TextMuted = Color.FromArgb(90, 90, 90);

        public static void ApplyForm(Form f)
        {
            f.Font = new Font("Segoe UI", 9.5F);
            f.BackColor = Bg;
            f.StartPosition = FormStartPosition.CenterScreen;
        }

        public static void StyleGrid(DataGridView dgv)
        {
            dgv.BackgroundColor = Color.White;
            dgv.BorderStyle = BorderStyle.FixedSingle;
            dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            dgv.EnableHeadersVisualStyles = false;
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 120, 215);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Padding = new Padding(4);
            dgv.ColumnHeadersHeight = 32;
            dgv.RowTemplate.Height = 26;
            dgv.DefaultCellStyle.Font = new Font("Segoe UI", 9.5F);
            dgv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(200, 220, 245);
            dgv.DefaultCellStyle.SelectionForeColor = Color.Black;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            dgv.AllowUserToAddRows = false;
            dgv.AllowUserToDeleteRows = false;
            dgv.ReadOnly = true;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgv.MultiSelect = false;
            dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgv.RowHeadersVisible = false;
        }

        public static void StylePrimaryButton(Button b)
        {
            b.BackColor = Primary;
            b.ForeColor = Color.White;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            b.Cursor = Cursors.Hand;
            b.Height = 32;
        }

        public static void StyleSecondaryButton(Button b)
        {
            b.BackColor = Color.FromArgb(230, 230, 230);
            b.ForeColor = Color.Black;
            b.FlatStyle = FlatStyle.Flat;
            b.FlatAppearance.BorderSize = 0;
            b.Font = new Font("Segoe UI", 9.5F);
            b.Cursor = Cursors.Hand;
            b.Height = 32;
        }

        public static void StyleTextBox(TextBox t)
        {
            t.BorderStyle = BorderStyle.FixedSingle;
            t.Font = new Font("Segoe UI", 10F);
        }

        /// <summary>
        /// Project-wide unsaved-changes prompt: Save / Don't Save / Cancel.
        /// </summary>
        public enum UnsavedChoice
        {
            Save = 0,
            DontSave = 1,
            Cancel = 2
        }

        public static UnsavedChoice ConfirmUnsavedChanges(IWin32Window owner, string title)
        {
            using (var dlg = new Form())
            {
                dlg.Text = string.IsNullOrEmpty(title) ? "Unsaved Changes" : title;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.ClientSize = new Size(360, 120);
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.ShowInTaskbar = false;
                dlg.Font = new Font("Segoe UI", 9.5F);

                var lbl = new Label();
                lbl.Text = "Save changes before continuing?";
                lbl.Location = new Point(16, 18);
                lbl.AutoSize = true;
                dlg.Controls.Add(lbl);

                var result = UnsavedChoice.Cancel;

                var btnSave = new Button();
                btnSave.Text = "Save";
                btnSave.Size = new Size(90, 28);
                btnSave.Location = new Point(50, 70);
                btnSave.Click += delegate { result = UnsavedChoice.Save; dlg.DialogResult = DialogResult.OK; dlg.Close(); };

                var btnDont = new Button();
                btnDont.Text = "Don't Save";
                btnDont.Size = new Size(100, 28);
                btnDont.Location = new Point(150, 70);
                btnDont.Click += delegate { result = UnsavedChoice.DontSave; dlg.DialogResult = DialogResult.OK; dlg.Close(); };

                var btnCancel = new Button();
                btnCancel.Text = "Cancel";
                btnCancel.Size = new Size(90, 28);
                btnCancel.Location = new Point(260, 70);
                btnCancel.Click += delegate { result = UnsavedChoice.Cancel; dlg.DialogResult = DialogResult.Cancel; dlg.Close(); };

                dlg.Controls.Add(btnSave);
                dlg.Controls.Add(btnDont);
                dlg.Controls.Add(btnCancel);
                dlg.CancelButton = btnCancel;
                dlg.AcceptButton = btnSave;

                if (owner != null)
                    dlg.ShowDialog(owner);
                else
                    dlg.ShowDialog();

                return result;
            }
        }

        public static bool ConfirmClose(Form f, string title)
        {
            return MessageBox.Show(
                "Are you sure you want to close " + title + "?",
                "Confirm Exit",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }

        public static bool ConfirmAppExit()
        {
            return MessageBox.Show(
                "Are you sure you want to exit PHARMA?",
                "Exit Application",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2) == DialogResult.Yes;
        }
    }
}
