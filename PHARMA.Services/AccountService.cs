using System.Collections.Generic;
using PHARMA.DataAccess.Repositories;
using PHARMA.Models;

namespace PHARMA.Services
{
    public class AccountService
    {
        private readonly AccountRepository _repo = new AccountRepository();

        public List<Account> Search(string term) { return _repo.Search(term); }
        public List<Account> SearchByType(string term, string partyType) { return _repo.SearchByType(term, partyType); }
        public Account Get(int acno) { return _repo.GetByCode(acno); }
        public int NextCode() { return _repo.GetNextCode(); }
        public bool IsReferenced(int acno) { return _repo.IsReferenced(acno); }
        public decimal OutstandingTotal() { return _repo.GetOutstandingTotal(); }

        public bool Save(Account a, out string error)
        {
            error = null;
            try
            {
                if (a == null)
                {
                    error = "Invalid account.";
                    return false;
                }
                if (a.acno <= 0)
                {
                    error = "Account code must be greater than zero.";
                    return false;
                }
                string name = !string.IsNullOrWhiteSpace(a.NAME) ? a.NAME.Trim() : (a.dsc ?? "").Trim();
                if (string.IsNullOrEmpty(name))
                {
                    error = "Account name is required.";
                    return false;
                }
                a.NAME = name;
                if (string.IsNullOrWhiteSpace(a.dsc)) a.dsc = name;
                if (string.IsNullOrEmpty(a.Partytype)) a.Partytype = "C";
                if (string.IsNullOrEmpty(a.StopTrans)) a.StopTrans = "N";

                var existing = _repo.GetByCode(a.acno);
                if (existing == null)
                    _repo.Insert(a);
                else
                {
                    a.Balance = existing.Balance;
                    _repo.Update(a);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool Deactivate(int acno, out string error)
        {
            error = null;
            try
            {
                var a = _repo.GetByCode(acno);
                if (a == null)
                {
                    error = "Account not found.";
                    return false;
                }
                a.StopTrans = "Y";
                _repo.Update(a);
                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool AdjustBalance(int acno, decimal amount, out string error)
        {
            error = null;
            try
            {
                if (_repo.GetByCode(acno) == null) { error = "Account not found."; return false; }
                _repo.AdjustBalance(acno, amount);
                return true;
            }
            catch (System.Exception ex) { error = ex.Message; return false; }
        }
    }
}
