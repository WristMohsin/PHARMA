using System.Collections.Generic;
using PHARMA.Models;

namespace PHARMA.DataAccess.Repositories
{
    public class ProductRepository : BaseRepository
    {
        public Product GetByCode(string pcode)
        {
            return QuerySingleOrDefault<Product>("SELECT * FROM product WHERE pcode = ?", new { pcode });
        }

        public Product GetByBarcode(string barcode)
        {
            return QuerySingleOrDefault<Product>("SELECT * FROM product WHERE BarCode1 = ? OR pcode = ?", new { barcode, pcode = barcode });
        }

        public IEnumerable<Product> Search(string term)
        {
            var sql = "SELECT TOP 50 * FROM product WHERE name1 LIKE ? OR pcode LIKE ? OR BarCode1 LIKE ? ORDER BY name1";
            var like = "%" + term + "%";
            return Query<Product>(sql, new { like, like2 = like, like3 = like });
        }

        public IEnumerable<Product> GetLowStock(int threshold = 10)
        {
            return Query<Product>("SELECT * FROM product WHERE balance <= ? AND Active = 'Y' ORDER BY balance", new { threshold });
        }

        public int GetStock(string pcode)
        {
            return ExecuteScalar<int>("SELECT ISNULL(balance, 0) FROM product WHERE pcode = ?", new { pcode });
        }
    }
}