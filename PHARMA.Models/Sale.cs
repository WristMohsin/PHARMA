using System;

namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.Sale
    /// </summary>
    public class Sale
    {
        public int invno { get; set; }
        public DateTime? invdt { get; set; }
        public int DocNo { get; set; }
        public int sno { get; set; }
        public int code { get; set; }
        public decimal grsamt { get; set; }
        public decimal disc { get; set; }
        public decimal xDip { get; set; }
        public decimal xdisc { get; set; }
        public decimal stax { get; set; }
        public decimal Net { get; set; }
        public decimal XDiscReturn { get; set; }
        public decimal SaleRt_Amt { get; set; }
        public decimal Creditnote_Amt { get; set; }
        public decimal Amt_Received { get; set; }
        public int type { get; set; }
        public string Remarks { get; set; }
        public int CreditDays { get; set; }
        public decimal PartyPRVBalance { get; set; }
        public int PrintCounter { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string Posted { get; set; }
        public decimal Amt1 { get; set; }
        public decimal amtr1 { get; set; }
        public string CounterPartyName { get; set; }
        public string PostTime { get; set; }
        public string CreditNote_No { get; set; }
        public int SummeryNo { get; set; }
        public string SummeryCode { get; set; }
        public string SummeryPosted { get; set; }
        public string SummeryClosed { get; set; }
        public decimal Xdisc3Amt { get; set; }
        public string WarrantyIssue { get; set; }
        public string Operator { get; set; }
        public int cash_return { get; set; }
        public string SrNO { get; set; }
        public string Computername { get; set; }
        public DateTime? MODIFYDATE { get; set; }
        public string retailinv { get; set; }
        public string cellno { get; set; }
        public int days { get; set; }
        public DateTime? ReminderDt { get; set; }
        public string BuiltyNo { get; set; }
        public DateTime? BuiltyDate { get; set; }
        public string msgSent { get; set; }
        public int shiftno { get; set; }
        public string OrderNo { get; set; }
        public DateTime? orderDt { get; set; }
        public decimal WHT { get; set; }
    }
}