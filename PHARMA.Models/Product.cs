using System;

namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.product
    /// </summary>
    public class Product
    {
        public string pcode { get; set; }
        public string PCODE1 { get; set; }
        public string OldCode { get; set; }
        public string CmpCd { get; set; }
        public string grcd { get; set; }
        public string LooseItem { get; set; }
        public string PackCode { get; set; }
        public string GrCd2 { get; set; }
        public string name1 { get; set; }
        public string shrtnm { get; set; }
        public string Desc1 { get; set; }
        public string pack { get; set; }
        public int unit { get; set; }
        public int percrtn { get; set; }
        public decimal tp { get; set; }
        public decimal rp { get; set; }
        public decimal dipTP { get; set; }
        public decimal DipRP { get; set; }
        public int GD1 { get; set; }
        public int GD2 { get; set; }
        public int GD3 { get; set; }
        public int GD4 { get; set; }
        public int GD5 { get; set; }
        public int balance { get; set; }
        public int hold_balance { get; set; }
        public int BalanceInLoose { get; set; }
        public string VALUEADDEDSTX { get; set; }
        public decimal StxP { get; set; }
        public decimal staxvalue { get; set; }
        public string DEFAULTGODOWN { get; set; }
        public decimal DipOnRetail { get; set; }
        public int SchemeQty { get; set; }
        public int SchemeBonus { get; set; }
        public decimal SchemeDisc1 { get; set; }
        public decimal schemedisc2 { get; set; }
        public decimal Mintp { get; set; }
        public string SRNO { get; set; }
        public string grcd3 { get; set; }
        public decimal Pur_Rate { get; set; }
        public string SaleOnTp { get; set; }
        public int GenericCode { get; set; }
        public int RACKORDER { get; set; }
        public string Active { get; set; }
        public int minOrder { get; set; }
        public int maxOrder { get; set; }
        public string StickerPrint { get; set; }
        public string BarCode1 { get; set; }
        public string StaxOn { get; set; }
        public string NamedItem { get; set; }
        public string updated { get; set; }
        public int StockOrderLevel { get; set; }
        public string Enabled { get; set; }
        public string mpost { get; set; }
        public string Godown { get; set; }
        public string NewProduct { get; set; }
        public string SteriodItem { get; set; }
        public string printname { get; set; }
        public int itemGroup { get; set; }
        public string ImportedItem { get; set; }
        public int CompanyCode { get; set; }
        public string Lock { get; set; }
        public string WHT { get; set; }
        public string pcodenew { get; set; }
    }
}