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
    }
}
