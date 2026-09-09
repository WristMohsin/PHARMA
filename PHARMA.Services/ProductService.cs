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
                var existing = _repo.GetByCode(p.pcode);
                if (existing == null) _repo.Insert(p);
                else _repo.Update(p);
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
