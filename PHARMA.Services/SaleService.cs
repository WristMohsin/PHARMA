using System;
using System.Collections.Generic;
using PHARMA.DataAccess.Repositories;
using PHARMA.Models;
using PHARMA.Common;

namespace PHARMA.Services
{
    public class SaleService
    {
        private readonly SaleRepository _saleRepo = new SaleRepository();
        private readonly ProductRepository _prodRepo = new ProductRepository();

        public int GetNextInvNo()
        {
            return _saleRepo.GetNextInvoiceNo();
        }

        public decimal GetTodayTotal()
        {
            return _saleRepo.GetTodaySaleTotal();
        }

        public Product FindProduct(string codeOrBarcode)
        {
            var p = _prodRepo.GetByBarcode(codeOrBarcode);
            if (p == null) p = _prodRepo.GetByCode(codeOrBarcode);
            return p;
        }

        public bool CanSell(string pcode, int qty, out string message)
        {
            message = null;
            var stock = _prodRepo.GetStock(pcode);
            if (stock < qty)
            {
                if (Constants.Stock.AllowNegativeStock)
                {
                    message = "Warning: Stock is " + stock + ". Allowing negative.";
                    return true;
                }
                message = "Insufficient stock. Available: " + stock;
                return false;
            }
            return true;
        }

        public bool SaveSale(Sale header, List<Sale_Detail> details, out string error)
        {
            error = null;
            try
            {
                if (details == null || details.Count == 0)
                {
                    error = "No items in sale.";
                    return false;
                }

                foreach (var d in details)
                {
                    string msg;
                    if (!CanSell(d.pcode, d.qty, out msg))
                    {
                        error = msg;
                        return false;
                    }
                }

                _saleRepo.InsertSale(header);
                foreach (var d in details)
                {
                    d.invno = header.invno;
                    d.invdt = header.invdt;
                    _saleRepo.InsertDetail(d);
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