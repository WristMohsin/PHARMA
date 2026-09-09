using System.Collections.Generic;
using PHARMA.DataAccess.Repositories;
using PHARMA.Models;

namespace PHARMA.Services
{
    public class ProductService
    {
        private readonly ProductRepository _repo = new ProductRepository();

        public Product Get(string pcode) => _repo.GetByCode(pcode);
        public IEnumerable<Product> Search(string term) => _repo.Search(term);
        public IEnumerable<Product> GetLowStock(int threshold = 10) => _repo.GetLowStock(threshold);
    }
}