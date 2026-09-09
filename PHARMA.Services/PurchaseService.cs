using System;
using System.Collections.Generic;
using PHARMA.DataAccess.Repositories;
using PHARMA.Models;

namespace PHARMA.Services
{
    public class PurchaseService
    {
        private readonly PurchaseRepository _repo = new PurchaseRepository();
        private readonly ProductRepository _prod = new ProductRepository();

        public int NextInvNo() { return _repo.GetNextInvNo(); }
        public List<purchase> Recent() { return _repo.GetRecent(); }
        public Product FindProduct(string code)
        {
            var p = _prod.GetByBarcode(code);
            if (p == null) p = _prod.GetByCode(code);
            return p;
        }

        public bool Save(purchase header, List<pur_det> details, out string error)
        {
            error = null;
            try
            {
                if (details == null || details.Count == 0) { error = "No items."; return false; }
                _repo.InsertHeader(header);
                foreach (var d in details)
                {
                    d.invno = header.invno;
                    d.invdt = header.invdt;
                    _repo.InsertDetail(d);
                    _prod.AdjustStock(d.pcode, d.qty);
                }
                return true;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }
    }
}
