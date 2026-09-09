using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.DataAccess.Repositories;
using PHARMA.UI.Helpers;

namespace PHARMA.UI.Forms.Masters
{
    public class CompanyListForm : Form
    {
        public CompanyListForm()
        {
            Text = "Companies";
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            BackColor = Color.FromArgb(250, 248, 240);
            FormClosing += (s, e) =>
            {
                if (!UiStyle.ConfirmClose(this, "Companies"))
                    e.Cancel = true;
            };
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };

            var top = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(245, 240, 225) };
            var lbl = new Label { Text = "Companies", Location = new Point(12, 12), Font = new Font("Segoe UI", 11F, FontStyle.Bold), AutoSize = true };
            var btnClose = new Button { Text = "Close (Esc)", Location = new Point(200, 8), Size = new Size(100, 28) };
            UiStyle.StyleSecondaryButton(btnClose);
            btnClose.Click += (s, e) => Close();
            top.Controls.Add(lbl);
            top.Controls.Add(btnClose);

            var dgv = new DataGridView { Dock = DockStyle.Fill };
            UiStyle.StyleGrid(dgv);
            dgv.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(180, 150, 80);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;

            Controls.Add(dgv);
            Controls.Add(top);

            try
            {
                var list = new CompanyRepository().GetAll();
                dgv.DataSource = list.Select(c => new
                {
                    Code = c.cmpcd,
                    Name = c.cmpnm != null ? c.cmpnm : c.CompanyName,
                    Address = c.CompanyAddress
                }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
