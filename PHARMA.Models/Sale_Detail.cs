using System;

namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.Sale_Detail
    /// </summary>
    public class Sale_Detail
    {
        public int Srno { get; set; }
        public int invno { get; set; }
        public DateTime? invdt { get; set; }
        public int SNO { get; set; }
        public int code { get; set; }
        public string pcode { get; set; }
        public decimal rate { get; set; }
        public decimal PcRt { get; set; }
        public int qty { get; set; }
        public string batchno { get; set; }
        public DateTime? expdt { get; set; }
        public int bonus { get; set; }
        public decimal dip { get; set; }
        public decimal dip2 { get; set; }
        public int QtyR { get; set; }
        public int BonusR { get; set; }
        public decimal StxPerItem { get; set; }
        public int type { get; set; }
        public string Market_Purchase { get; set; }
        public string MarketPur_Posting { get; set; }
        public string SaleOnTp { get; set; }
        public int Pur_Srno { get; set; }
        public int Pur_Invno { get; set; }
        public int SortNo { get; set; }
        public decimal WHTAmt { get; set; }
        public decimal WHTRate { get; set; }
        public string iColorName { get; set; }
        public DateTime? Exp_Date2 { get; set; }
        public string batchNo2 { get; set; }
        public string pcode2 { get; set; }
        public decimal rate2 { get; set; }
        public int Qty2 { get; set; }
        public decimal RP { get; set; }
    }
}