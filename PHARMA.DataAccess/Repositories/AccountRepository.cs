using System.Collections.Generic;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class AccountRepository : BaseRepository
    {
        public List<Account> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Query<Account>("SELECT TOP 100 * FROM ACCOUNT ORDER BY NAME");
            var like = "%" + term + "%";
            return Query<Account>(
                "SELECT TOP 100 * FROM ACCOUNT WHERE NAME LIKE ? OR dsc LIKE ? OR Mobile LIKE ? ORDER BY NAME",
                like, like, like);
        }

        public Account GetByCode(int acno)
        {
            return QuerySingleOrDefault<Account>("SELECT * FROM ACCOUNT WHERE acno = ?", acno);
        }

        public int GetNextCode()
        {
            return ExecuteScalar<int>("SELECT ISNULL(MAX(acno), 0) + 1 FROM ACCOUNT");
        }

        public int Insert(Account a)
        {
            return Execute(
                @"INSERT INTO ACCOUNT (acno, dsc, NAME, Address, Phone, Mobile, AreaCd, Balance, Partytype, StopTrans)
                  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                a.acno, a.dsc, a.NAME, a.Address, a.Phone, a.Mobile, a.AreaCd, a.Balance,
                a.Partytype ?? "C", a.StopTrans ?? "N");
        }

        public int Update(Account a)
        {
            return Execute(
                @"UPDATE ACCOUNT SET dsc=?, NAME=?, Address=?, Phone=?, Mobile=?, AreaCd=?, Balance=?, Partytype=?
                  WHERE acno=?",
                a.dsc, a.NAME, a.Address, a.Phone, a.Mobile, a.AreaCd, a.Balance, a.Partytype, a.acno);
        }

        public decimal GetOutstandingTotal()
        {
            return ExecuteScalar<decimal>("SELECT ISNULL(SUM(Balance), 0) FROM ACCOUNT WHERE Balance > 0");
        }
    }
}
