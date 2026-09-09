using System;

namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.purchase
    /// </summary>
    public class purchase
    {
        public int invno { get; set; }
        public DateTime? invdt { get; set; }
        public int code { get; set; }
        public string docno { get; set; }
        public DateTime? docdt { get; set; }
        public decimal grsamt { get; set; }
        public decimal disc { get; set; }
        public decimal xdisc { get; set; }
        public decimal stax { get; set; }
        public decimal Freight { get; set; }
        public decimal net { get; set; }
        public int type { get; set; }
        public string remarks { get; set; }
        public decimal disc2amt { get; set; }
        public decimal CashPaid { get; set; }
        public decimal exponPurchase { get; set; }
        public string Operator { get; set; }
        public string PostTime { get; set; }
        public string ComputerName { get; set; }
        public DateTime? MODIFYDATE { get; set; }
        public int medRepCD { get; set; }
        public int shiftno { get; set; }
        public int sno { get; set; }
        public decimal WHT { get; set; }
        public string createTime { get; set; }
    }
}