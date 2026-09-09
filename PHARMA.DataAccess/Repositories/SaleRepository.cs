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

        public Sale GetByInvNo(int invno)
        {
            return QuerySingleOrDefault<Sale>("SELECT * FROM Sale WHERE invno = ?", invno);
        }

        public List<Sale_Detail> GetDetails(int invno)
        {
            return Query<Sale_Detail>("SELECT * FROM Sale_Detail WHERE invno = ? ORDER BY SortNo, Srno", invno);
        }

        public decimal GetTodaySaleTotal()
        {
            return ExecuteScalar<decimal>(
                "SELECT ISNULL(SUM(Net), 0) FROM Sale WHERE CAST(invdt AS DATE) = CAST(GETDATE() AS DATE)");
        }

        public int InsertSale(Sale s)
        {
            var sql = @"
INSERT INTO Sale (invno, invdt, DocNo, sno, code, grsamt, disc, xDip, xdisc, stax, Net, 
                 XDiscReturn, SaleRt_Amt, Creditnote_Amt, Amt_Received, type, Remarks, 
                 CreditDays, PartyPRVBalance, PrintCounter, Posted, CounterPartyName, 
                 PostTime, Operator, Computername, shiftno, WHT)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
            return Execute(sql,
                s.invno, s.invdt, s.DocNo, s.sno, s.code, s.grsamt, s.disc, s.xDip, s.xdisc, s.stax, s.Net,
                s.XDiscReturn, s.SaleRt_Amt, s.Creditnote_Amt, s.Amt_Received, s.type, s.Remarks,
                s.CreditDays, s.PartyPRVBalance, s.PrintCounter, s.Posted, s.CounterPartyName,
                s.PostTime, s.Operator, s.Computername, s.shiftno, s.WHT);
        }

        public int InsertDetail(Sale_Detail d)
        {
            var sql = @"
INSERT INTO Sale_Detail (invno, invdt, SNO, code, pcode, rate, PcRt, qty, batchno, expdt, 
                         bonus, dip, dip2, QtyR, BonusR, StxPerItem, type, SaleOnTp, SortNo)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
            return Execute(sql,
                d.invno, d.invdt, d.SNO, d.code, d.pcode, d.rate, d.PcRt, d.qty, d.batchno, d.expdt,
                d.bonus, d.dip, d.dip2, d.QtyR, d.BonusR, d.StxPerItem, d.type, d.SaleOnTp, d.SortNo);
        }
    }
}