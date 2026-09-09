using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Services;
using PHARMA.Models;
using PHARMA.UI.Helpers;

namespace PHARMA.UI.Forms.Sale
{
    public class SaleListForm : Form
    {
        private readonly SaleService _svc = new SaleService();
        private List<PHARMA.Models.Sale> _all = new List<PHARMA.Models.Sale>();
        private TextBox txtSearch;
        private DataGridView dgv;
        private DateTimePicker dtFrom, dtTo;
        private Label lblInfo;

        public SaleListForm()
        {
            Text = "Sale History";
            WindowState = FormWindowState.Maximized;
            KeyPreview = true;
            UiStyle.ApplyForm(this);
            BuildUI();
            LoadData();
            FormClosing += (s, e) =>
            {
                if (!UiStyle.ConfirmClose(this, "Sale History"))
                    e.Cancel = true;
            };
            KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) Close(); };
        }

        private void BuildUI()
        {
            var top = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Color.White };

            var lbl1 = new Label { Text = "From", Location = new Point(12, 16), AutoSize = true };
            dtFrom = new DateTimePicker { Location = new Point(55, 12), Width = 120, Value = DateTime.Today.AddDays(-7) };
            var lbl2 = new Label { Text = "To", Location = new Point(190, 16), AutoSize = true };
            dtTo = new DateTimePicker { Location = new Point(220, 12), Width = 120, Value = DateTime.Today };

            var lblS = new Label { Text = "Filter", Location = new Point(360, 16), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(405, 12), Size = new Size(160, 28) };
            UiStyle.StyleTextBox(txtSearch);
            txtSearch.TextChanged += (s, e) => ApplyFilter();

            var btn = new Button { Text = "Search", Location = new Point(580, 11), Size = new Size(80, 32) };
            UiStyle.StylePrimaryButton(btn);
            btn.Click += (s, e) => LoadData();

            var btnClose = new Button { Text = "Close", Location = new Point(670, 11), Size = new Size(80, 32) };
            UiStyle.StyleSecondaryButton(btnClose);
            btnClose.Click += (s, e) => Close();

            top.Controls.AddRange(new Control[] { lbl1, dtFrom, lbl2, dtTo, lblS, txtSearch, btn, btnClose });

            lblInfo = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                BackColor = Color.FromArgb(235, 238, 242)
            };

            dgv = new DataGridView { Dock = DockStyle.Fill };
            UiStyle.StyleGrid(dgv);

            Controls.Add(dgv);
            Controls.Add(lblInfo);
            Controls.Add(top);
        }

        private void LoadData()
        {
            try
            {
                _all = _svc.SearchByDate(dtFrom.Value.Date, dtTo.Value.Date);
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Sale History", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyFilter()
        {
            string q = (txtSearch.Text ?? "").Trim().ToLowerInvariant();
            IEnumerable<PHARMA.Models.Sale> list = _all;
            if (!string.IsNullOrEmpty(q))
            {
                list = _all.Where(s =>
                    s.invno.ToString().Contains(q) ||
                    s.code.ToString().Contains(q) ||
                    (s.Operator != null && s.Operator.ToLowerInvariant().Contains(q)));
            }
            var rows = list.Select(s => new
            {
                Invoice = s.invno,
                Date = s.invdt,
                Party = s.code,
                Gross = s.grsamt,
                Disc = s.disc,
                Net = s.Net,
                Operator = s.Operator
            }).ToList();
            dgv.DataSource = rows;
            decimal total = 0;
            foreach (var x in list) total += x.Net;
            lblInfo.Text = "Invoices: " + rows.Count + "  |  Total Net: " + total.ToString("N2") + "  |  Esc=Close";
        }
    }
}
