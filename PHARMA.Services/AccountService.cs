using System.Collections.Generic;
using PHARMA.DataAccess.Repositories;
using PHARMA.Models;

namespace PHARMA.Services
{
    public class AccountService
    {
        private readonly AccountRepository _repo = new AccountRepository();
        public List<Account> Search(string term) { return _repo.Search(term); }
        public Account Get(int acno) { return _repo.GetByCode(acno); }
        public int NextCode() { return _repo.GetNextCode(); }
        public bool Save(Account a, out string error)
        {
            error = null;
            try
            {
                if (a.acno <= 0) a.acno = _repo.GetNextCode();
                var existing = _repo.GetByCode(a.acno);
                if (existing == null) _repo.Insert(a); else _repo.Update(a);
                return true;
            }
            catch (System.Exception ex) { error = ex.Message; return false; }
        }
        public decimal OutstandingTotal() { return _repo.GetOutstandingTotal(); }

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
