using System.Collections.Generic;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class PurchaseRepository : BaseRepository
    {
        public int GetNextInvNo()
        {
            return ExecuteScalar<int>("SELECT ISNULL(MAX(invno), 0) + 1 FROM purchase");
        }

        public List<purchase> GetRecent(int top = 50)
        {
            return Query<purchase>("SELECT TOP " + top + " * FROM purchase ORDER BY invno DESC");
        }

        public purchase GetByInvNo(int invno)
        {
            return QuerySingleOrDefault<purchase>("SELECT * FROM purchase WHERE invno = ?", invno);
        }

        public List<pur_det> GetDetails(int invno)
        {
            return Query<pur_det>("SELECT * FROM pur_det WHERE invno = ? ORDER BY Srno", invno);
        }

        public int InsertHeader(purchase p)
        {
            return Execute(
                @"INSERT INTO purchase (invno, invdt, code, docno, docdt, grsamt, disc, xdisc, stax, Freight, net, type, remarks, Operator, ComputerName, shiftno, WHT)
                  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                p.invno, p.invdt, p.code, p.docno, p.docdt, p.grsamt, p.disc, p.xdisc, p.stax, p.Freight, p.net,
                p.type, p.remarks, p.Operator, p.ComputerName, p.shiftno, p.WHT);
        }

        public int InsertDetail(pur_det d)
        {
            return Execute(
                @"INSERT INTO pur_det (invno, invdt, code, pcode, rate, PcRt, qty, bonus, batchno, expdt, dip, dip2, stxPerItem, type, SortNo)
                  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                d.invno, d.invdt, d.code, d.pcode, d.rate, d.PcRt, d.qty, d.bonus, d.batchno, d.expdt,
                d.dip, d.dip2, d.stxPerItem, d.type, d.SortNo);
        }
    }
}
