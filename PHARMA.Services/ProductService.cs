using System.Collections.Generic;
using PHARMA.DataAccess.Repositories;
using PHARMA.Models;

namespace PHARMA.Services
{
    public class ProductService
    {
        private readonly ProductRepository _repo = new ProductRepository();

        public Product Get(string pcode) { return _repo.GetByCode(pcode); }
        public Product GetByBarcode(string bc) { return _repo.GetByBarcode(bc); }
        public List<Product> Search(string term) { return _repo.Search(term); }
        public List<Product> GetLowStock(int threshold) { return _repo.GetLowStock(threshold); }
        public int GetStock(string pcode) { return _repo.GetStock(pcode); }

        public bool Save(Product p, out string error)
        {
            error = null;
            try
            {
                if (p == null)
                {
                    error = "Invalid product.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(p.pcode))
                {
                    error = "Product code required.";
                    return false;
                }
                if (string.IsNullOrWhiteSpace(p.name1))
                {
                    error = "Product name required.";
                    return false;
                }
                if (p.unit <= 0) p.unit = 1;
                if (p.tp < 0 || p.rp < 0 || p.Pur_Rate < 0)
                {
                    error = "Prices cannot be negative.";
                    return false;
                }
                if (string.IsNullOrEmpty(p.Active)) p.Active = "Y";

                p.pcode = p.pcode.Trim();
                p.name1 = p.name1.Trim();

                var existing = _repo.GetByCode(p.pcode);
                if (existing == null)
                {
                    _repo.Insert(p);
                }
                else
                {
                    p.balance = existing.balance;
                    _repo.Update(p);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool Deactivate(string pcode, out string error)
        {
            error = null;
            try
            {
                var p = _repo.GetByCode(pcode);
                if (p == null)
                {
                    error = "Product not found.";
                    return false;
                }
                p.Active = "N";
                _repo.Update(p);
                return true;
            }
            catch (System.Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
