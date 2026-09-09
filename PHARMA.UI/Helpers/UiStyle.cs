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
