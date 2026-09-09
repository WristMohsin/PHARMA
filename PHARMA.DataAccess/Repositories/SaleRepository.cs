using System;
using System.Collections.Generic;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class SaleRepository : BaseRepository
    {
        public int GetNextInvoiceNo()
        {
            return ExecuteScalar<int>("SELECT ISNULL(MAX(invno), 0) + 1 FROM Sale");
        }

        public decimal GetTodaySaleTotal()
        {
            return ExecuteScalar<decimal>(
                "SELECT ISNULL(SUM(Net), 0) FROM Sale WHERE CONVERT(date, invdt) = CONVERT(date, GETDATE())");
        }

        public List<Sale> GetRecent(int top)
        {
            return Query<Sale>("SELECT TOP " + top + " * FROM Sale ORDER BY invno DESC");
        }

        public List<Sale> SearchByDate(DateTime from, DateTime to)
        {
            return Query<Sale>(
                "SELECT * FROM Sale WHERE invdt >= ? AND invdt < ? ORDER BY invno DESC",
                from.Date, to.Date.AddDays(1));
        }

        public Sale GetByInvNo(int invno)
        {
            return QuerySingleOrDefault<Sale>("SELECT * FROM Sale WHERE invno = ?", invno);
        }

        public List<Sale_Detail> GetDetails(int invno)
        {
            return Query<Sale_Detail>("SELECT * FROM Sale_Detail WHERE invno = ? ORDER BY SortNo, Srno", invno);
        }

        public int InsertSale(Sale s)
        {
            return Execute(
                @"INSERT INTO Sale (invno, invdt, code, grsamt, disc, xdisc, stax, Net, type, Remarks, Operator, Computername, Posted, PostTime, shiftno, WHT, OrderNo)
                  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                s.invno, s.invdt, s.code, s.grsamt, s.disc, s.xdisc, s.stax, s.Net, s.type,
                s.Remarks, s.Operator, s.Computername, s.Posted, s.PostTime, s.shiftno, s.WHT, s.OrderNo);
        }

        public int InsertDetail(Sale_Detail d)
        {
            return Execute(
                @"INSERT INTO Sale_Detail (invno, invdt, code, pcode, rate, qty, bonus, batchno, expdt, dip, dip2, type, SortNo)
                  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                d.invno, d.invdt, d.code, d.pcode, d.rate, d.qty, d.bonus, d.batchno, d.expdt, d.dip, d.dip2, d.type, d.SortNo);
        }
    }
}
