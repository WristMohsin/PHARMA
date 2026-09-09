using System.Collections.Generic;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class ProductRepository : BaseRepository
    {
        public Product GetByCode(string pcode)
        {
            return QuerySingleOrDefault<Product>("SELECT * FROM product WHERE pcode = ?", pcode);
        }

        public Product GetByBarcode(string barcode)
        {
            return QuerySingleOrDefault<Product>(
                "SELECT * FROM product WHERE BarCode1 = ? OR pcode = ?", barcode, barcode);
        }

        public List<Product> Search(string term)
        {
            var like = "%" + term + "%";
            return Query<Product>(
                "SELECT TOP 50 * FROM product WHERE name1 LIKE ? OR pcode LIKE ? OR BarCode1 LIKE ? ORDER BY name1",
                like, like, like);
        }

        public List<Product> GetLowStock(int threshold = 10)
        {
            return Query<Product>(
                "SELECT * FROM product WHERE balance <= ? AND Active = 'Y' ORDER BY balance", threshold);
        }

        public int GetStock(string pcode)
        {
            return ExecuteScalar<int>("SELECT ISNULL(balance, 0) FROM product WHERE pcode = ?", pcode);
        }
    }
}