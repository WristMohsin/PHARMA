using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Services;

namespace PHARMA.UI.Forms.Sale
{
    public class SaleListForm : Form
    {
        private readonly SaleService _svc = new SaleService();
        private DataGridView dgv;
        private DateTimePicker dtFrom, dtTo;
        private Label lblInfo;

        public SaleListForm()
        {
            Text = "Sale History";
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            BuildUI();
            LoadData();
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        }

        private void BuildUI()
        {
            var lbl1 = new Label { Text = "From", Location = new Point(20, 15), AutoSize = true };
            dtFrom = new DateTimePicker { Location = new Point(60, 12), Width = 120, Value = DateTime.Today.AddDays(-7) };
            var lbl2 = new Label { Text = "To", Location = new Point(200, 15), AutoSize = true };
            dtTo = new DateTimePicker { Location = new Point(230, 12), Width = 120, Value = DateTime.Today };

            var btn = new Button { Text = "Search", Location = new Point(370, 10), Size = new Size(90, 30) };
            btn.Click += (s, e) => LoadData();
            var btnClose = new Button { Text = "Close (Esc)", Location = new Point(470, 10), Size = new Size(100, 30) };
            btnClose.Click += (s, e) => Close();

            lblInfo = new Label { Location = new Point(20, 50), AutoSize = true };
            dgv = new DataGridView
            {
                Location = new Point(20, 75),
                Size = new Size(900, 450),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                ReadOnly = true,
                AllowUserToAddRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            Controls.AddRange(new Control[] { lbl1, dtFrom, lbl2, dtTo, btn, btnClose, lblInfo, dgv });
        }

        private void LoadData()
        {
            try
            {
                var list = _svc.SearchByDate(dtFrom.Value.Date, dtTo.Value.Date);
                dgv.DataSource = list.Select(s => new
                {
                    Invoice = s.invno,
                    Date = s.invdt,
                    Party = s.code,
                    Gross = s.grsamt,
                    Disc = s.disc,
                    Net = s.Net,
                    Operator = s.Operator
                }).ToList();
                decimal total = 0;
                foreach (var x in list) total += x.Net;
                lblInfo.Text = "Invoices: " + list.Count + "  |  Total Net: " + total.ToString("N2");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
