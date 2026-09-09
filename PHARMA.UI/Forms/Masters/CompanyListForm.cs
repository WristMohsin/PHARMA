using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.DataAccess.Repositories;

namespace PHARMA.UI.Forms.Masters
{
    public class CompanyListForm : Form
    {
        public CompanyListForm()
        {
            Text = "Companies";
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            var dgv = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Controls.Add(dgv);
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
            try
            {
                var list = new CompanyRepository().GetAll();
                dgv.DataSource = list.Select(c => new { Code = c.cmpcd, Name = c.cmpnm ?? c.CompanyName, c.CompanyAddress }).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
