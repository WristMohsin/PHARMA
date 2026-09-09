using System;

namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.Batch
    /// </summary>
    public class Batch
    {
        public int SRNO { get; set; }
        public string batchno { get; set; }
        public string pcode { get; set; }
        public DateTime? expdt { get; set; }
        public decimal rate { get; set; }
        public int qty { get; set; }
        public string Godown { get; set; }
        public int bonus { get; set; }
        public decimal dip { get; set; }
        public decimal pcRt { get; set; }
        public int balance { get; set; }
        public int BalanceInUnit { get; set; }
        public int pur_No { get; set; }
        public DateTime? IntimationDate { get; set; }
    }
}