using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using PHARMA.Models;

namespace PHARMA.UI.Helpers
{
    public class ReceiptPrinter
    {
        private Sale _header;
        private List<Sale_Detail> _details;
        private string _partyName;

        public void Print(Sale header, List<Sale_Detail> details, string partyName)
        {
            _header = header;
            _details = details;
            _partyName = partyName ?? "";
            var doc = new PrintDocument();
            doc.PrintPage += Doc_PrintPage;
            try
            {
                var preview = new PrintPreviewDialog();
                preview.Document = doc;
                preview.Width = 800;
                preview.Height = 600;
                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Print error: " + ex.Message);
            }
        }

        private void Doc_PrintPage(object sender, PrintPageEventArgs e)
        {
            float y = 20;
            var fontTitle = new Font("Arial", 14, FontStyle.Bold);
            var font = new Font("Consolas", 9);
            var fontBold = new Font("Consolas", 9, FontStyle.Bold);
            var g = e.Graphics;

            g.DrawString("PHARMA / PharmaZ", fontTitle, Brushes.Black, 20, y); y += 28;
            g.DrawString("Sale Invoice", fontBold, Brushes.Black, 20, y); y += 20;
            g.DrawString("Invoice #: " + _header.invno, font, Brushes.Black, 20, y); y += 16;
            g.DrawString("Date: " + (_header.invdt.HasValue ? _header.invdt.Value.ToString("dd-MMM-yyyy HH:mm") : ""), font, Brushes.Black, 20, y); y += 16;
            if (!string.IsNullOrEmpty(_partyName))
            {
                g.DrawString("Party: " + _partyName, font, Brushes.Black, 20, y); y += 16;
            }
            g.DrawString("----------------------------------------", font, Brushes.Black, 20, y); y += 16;
            g.DrawString(string.Format("{0,-12} {1,5} {2,8} {3,10}", "Code", "Qty", "Rate", "Amount"), fontBold, Brushes.Black, 20, y); y += 16;
            g.DrawString("----------------------------------------", font, Brushes.Black, 20, y); y += 16;

            foreach (var d in _details)
            {
                decimal amt = d.qty * d.rate;
                g.DrawString(string.Format("{0,-12} {1,5} {2,8:N2} {3,10:N2}", d.pcode, d.qty, d.rate, amt), font, Brushes.Black, 20, y);
                y += 15;
            }
            g.DrawString("----------------------------------------", font, Brushes.Black, 20, y); y += 16;
            g.DrawString(string.Format("Gross: {0:N2}", _header.grsamt), font, Brushes.Black, 20, y); y += 15;
            if (_header.disc > 0)
            {
                g.DrawString(string.Format("Discount: {0:N2}", _header.disc), font, Brushes.Black, 20, y); y += 15;
            }
            g.DrawString(string.Format("NET TOTAL: {0:N2}", _header.Net), fontBold, Brushes.Black, 20, y); y += 20;
            g.DrawString("Operator: " + (_header.Operator ?? ""), font, Brushes.Black, 20, y); y += 15;
            g.DrawString("Thank you!", font, Brushes.Black, 20, y);
            e.HasMorePages = false;
        }
    }
}
