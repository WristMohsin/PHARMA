using System;

namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.pur_det
    /// </summary>
    public class pur_det
    {
        public int Srno { get; set; }
        public int invno { get; set; }
        public DateTime? invdt { get; set; }
        public int code { get; set; }
        public string pcode { get; set; }
        public decimal rate { get; set; }
        public decimal PcRt { get; set; }
        public int qty { get; set; }
        public int bonus { get; set; }
        public int QtyIssued { get; set; }
        public string batchno { get; set; }
        public DateTime? expdt { get; set; }
        public decimal dip { get; set; }
        public decimal dip2 { get; set; }
        public decimal stxPerItem { get; set; }
        public int type { get; set; }
        public int StockType { get; set; }
        public int SortNo { get; set; }
        public decimal WHTAmt { get; set; }
        public decimal WHTRate { get; set; }
    }
}