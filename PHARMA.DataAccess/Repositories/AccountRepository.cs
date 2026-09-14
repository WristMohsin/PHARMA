using System.Collections.Generic;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class AccountRepository : BaseRepository
    {
        public List<Account> Search(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Query<Account>("SELECT TOP 200 * FROM ACCOUNT ORDER BY acno");
            var like = "%" + term.Trim() + "%";
            return Query<Account>(
                "SELECT TOP 150 * FROM ACCOUNT WHERE NAME LIKE ? OR dsc LIKE ? OR CAST(acno AS VARCHAR(20)) LIKE ? OR Mobile LIKE ? ORDER BY acno",
                like, like, like, like);
        }

        public List<Account> SearchByType(string term, string partyType)
        {
            var like = string.IsNullOrWhiteSpace(term) ? null : ("%" + term.Trim() + "%");
            if (string.IsNullOrWhiteSpace(partyType) || partyType == "*")
            {
                if (like == null)
                    return Query<Account>("SELECT TOP 200 * FROM ACCOUNT ORDER BY acno");
                return Query<Account>(
                    "SELECT TOP 150 * FROM ACCOUNT WHERE NAME LIKE ? OR dsc LIKE ? OR CAST(acno AS VARCHAR(20)) LIKE ? ORDER BY acno",
                    like, like, like);
            }
            if (like == null)
                return Query<Account>(
                    "SELECT TOP 150 * FROM ACCOUNT WHERE ISNULL(Partytype,'') = ? ORDER BY acno",
                    partyType);
            return Query<Account>(
                "SELECT TOP 150 * FROM ACCOUNT WHERE ISNULL(Partytype,'') = ? AND (NAME LIKE ? OR dsc LIKE ? OR CAST(acno AS VARCHAR(20)) LIKE ?) ORDER BY acno",
                partyType, like, like, like);
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
                @"INSERT INTO ACCOUNT (acno, Scode, SSHcd, dsc, NAME, Address, Phone, Mobile, AreaCd, Balance, Partytype, StopTrans, SysAc, Main)
                  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                a.acno, a.Scode, a.SSHcd, a.dsc, a.NAME, a.Address, a.Phone, a.Mobile, a.AreaCd, a.Balance,
                string.IsNullOrEmpty(a.Partytype) ? "C" : a.Partytype,
                string.IsNullOrEmpty(a.StopTrans) ? "N" : a.StopTrans,
                string.IsNullOrEmpty(a.SysAc) ? " " : a.SysAc,
                a.Main);
        }

        public int Update(Account a)
        {
            return Execute(
                @"UPDATE ACCOUNT SET Scode=?, SSHcd=?, dsc=?, NAME=?, Address=?, Phone=?, Mobile=?, AreaCd=?,
                  Partytype=?, StopTrans=?, SysAc=?, Main=?
                  WHERE acno=?",
                a.Scode, a.SSHcd, a.dsc, a.NAME, a.Address, a.Phone, a.Mobile, a.AreaCd,
                a.Partytype, a.StopTrans, a.SysAc, a.Main, a.acno);
        }

        public int AdjustBalance(int acno, decimal amount)
        {
            return Execute("UPDATE ACCOUNT SET Balance = ISNULL(Balance, 0) + ? WHERE acno = ?", amount, acno);
        }

        public decimal GetOutstandingTotal()
        {
            return ExecuteScalar<decimal>("SELECT ISNULL(SUM(Balance), 0) FROM ACCOUNT WHERE Balance > 0");
        }

        public bool IsReferenced(int acno)
        {
            int sale = ExecuteScalar<int>("SELECT ISNULL(COUNT(*),0) FROM Sale WHERE code = ?", acno);
            if (sale > 0) return true;
            int pur = ExecuteScalar<int>("SELECT ISNULL(COUNT(*),0) FROM purchase WHERE code = ?", acno);
            if (pur > 0) return true;
            try
            {
                int led = ExecuteScalar<int>("SELECT ISNULL(COUNT(*),0) FROM DetailLedger WHERE code = ?", acno);
                if (led > 0) return true;
            }
            catch
            {
            }
            return false;
        }
    }
}
