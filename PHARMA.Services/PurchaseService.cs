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
        public purchase Get(int invno) { return _repo.GetByInvNo(invno); }
        public List<pur_det> GetDetails(int invno) { return _repo.GetDetails(invno); }

        public Product FindProduct(string codeOrBarcode)
        {
            if (string.IsNullOrEmpty(codeOrBarcode)) return null;
            var p = _prod.GetByBarcode(codeOrBarcode);
            if (p == null) p = _prod.GetByCode(codeOrBarcode);
            return p;
        }

        public bool Save(purchase header, List<pur_det> details, out string error)
        {
            error = null;
            try
            {
                if (header == null)
                {
                    error = "Invalid purchase header.";
                    return false;
                }
                if (details == null || details.Count == 0)
                {
                    error = "No items in purchase.";
                    return false;
                }

                foreach (var d in details)
                {
                    if (string.IsNullOrEmpty(d.pcode))
                    {
                        error = "A line is missing product code.";
                        return false;
                    }
                    if (d.qty <= 0)
                    {
                        error = "Quantity must be greater than zero for product " + d.pcode + ".";
                        return false;
                    }
                    if (d.rate < 0)
                    {
                        error = "Rate cannot be negative for product " + d.pcode + ".";
                        return false;
                    }
                }

                if (header.invno <= 0)
                    header.invno = _repo.GetNextInvNo();
                if (header.invdt == null)
                    header.invdt = DateTime.Now;

                _repo.InsertHeader(header);

                int sort = 1;
                foreach (var d in details)
                {
                    d.invno = header.invno;
                    d.invdt = header.invdt;
                    d.code = header.code;
                    if (d.SortNo <= 0) d.SortNo = sort;
                    sort++;
                    if (d.type == 0) d.type = 1;

                    _repo.InsertDetail(d);
                    _prod.AdjustStock(d.pcode, d.qty);
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
