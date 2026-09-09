using System;
using System.Collections.Generic;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class SaleRepository : BaseRepository
    {
        public int GetNextInvoiceNo()
        {
            var sql = "SELECT ISNULL(MAX(invno), 0) + 1 FROM Sale";
            return ExecuteScalar<int>(sql);
        }

        public Sale GetByInvNo(int invno)
        {
            return QuerySingleOrDefault<Sale>("SELECT * FROM Sale WHERE invno = ?", new { invno });
        }

        public IEnumerable<Sale_Detail> GetDetails(int invno)
        {
            return Query<Sale_Detail>("SELECT * FROM Sale_Detail WHERE invno = ? ORDER BY SortNo, Srno", new { invno });
        }

        public decimal GetTodaySaleTotal()
        {
            var sql = "SELECT ISNULL(SUM(Net), 0) FROM Sale WHERE CAST(invdt AS DATE) = CAST(GETDATE() AS DATE)";
            return ExecuteScalar<decimal>(sql);
        }

        public int InsertSale(Sale sale)
        {
            var sql = @"
INSERT INTO Sale (invno, invdt, DocNo, sno, code, grsamt, disc, xDip, xdisc, stax, Net, 
                 XDiscReturn, SaleRt_Amt, Creditnote_Amt, Amt_Received, type, Remarks, 
                 CreditDays, PartyPRVBalance, PrintCounter, Posted, CounterPartyName, 
                 PostTime, Operator, Computername, shiftno, WHT)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
            return Execute(sql, sale);
        }

        public int InsertDetail(Sale_Detail d)
        {
            var sql = @"
INSERT INTO Sale_Detail (invno, invdt, SNO, code, pcode, rate, PcRt, qty, batchno, expdt, 
                         bonus, dip, dip2, QtyR, BonusR, StxPerItem, type, SaleOnTp, SortNo)
VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
            return Execute(sql, d);
        }
    }
}