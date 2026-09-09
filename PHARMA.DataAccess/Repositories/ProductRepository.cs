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
            if (string.IsNullOrWhiteSpace(term) || term == "%")
                return Query<Product>("SELECT TOP 200 * FROM product ORDER BY name1");
            var like = "%" + term.Trim() + "%";
            return Query<Product>(
                "SELECT TOP 100 * FROM product WHERE name1 LIKE ? OR pcode LIKE ? OR BarCode1 LIKE ? ORDER BY name1",
                like, like, like);
        }

        public List<Product> GetLowStock(int threshold)
        {
            return Query<Product>(
                "SELECT * FROM product WHERE balance <= ? AND (Active = 'Y' OR Active IS NULL OR Active = '') ORDER BY balance", threshold);
        }

        public int GetStock(string pcode)
        {
            return ExecuteScalar<int>("SELECT ISNULL(balance, 0) FROM product WHERE pcode = ?", pcode);
        }

        public int Insert(Product p)
        {
            return Execute(
                @"INSERT INTO product (pcode, name1, pack, unit, tp, rp, Pur_Rate, balance, BarCode1, Active, CmpCd)
                  VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)",
                p.pcode, p.name1, p.pack, p.unit, p.tp, p.rp, p.Pur_Rate, p.balance, p.BarCode1,
                string.IsNullOrEmpty(p.Active) ? "Y" : p.Active, p.CmpCd);
        }

        public int Update(Product p)
        {
            return Execute(
                @"UPDATE product SET name1=?, pack=?, unit=?, tp=?, rp=?, Pur_Rate=?, balance=?, BarCode1=?, Active=?, CmpCd=?
                  WHERE pcode=?",
                p.name1, p.pack, p.unit, p.tp, p.rp, p.Pur_Rate, p.balance, p.BarCode1, p.Active, p.CmpCd, p.pcode);
        }
    }
}
