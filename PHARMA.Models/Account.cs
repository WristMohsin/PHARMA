using System;

namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.ACCOUNT
    /// </summary>
    public class Account
    {
        public int acno { get; set; }
        public int Scode { get; set; }
        public int SSHcd { get; set; }
        public string dsc { get; set; }
        public string Address { get; set; }
        public string PostalAddress { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public int CrLimit { get; set; }
        public string PartyMod { get; set; }
        public string StxNo { get; set; }
        public int AreaCd { get; set; }
        public decimal Balance { get; set; }
        public string NAME { get; set; }
        public string LicNo { get; set; }
        public DateTime? LicExpDT { get; set; }
        public string StopTrans { get; set; }
        public string warranty { get; set; }
        public string SysAc { get; set; }
        public string AreacdParty { get; set; }
        public string OldCode { get; set; }
        public string Main { get; set; }
        public string distCD { get; set; }
        public string Partytype { get; set; }
        public string mpost { get; set; }
        public string ExpAC { get; set; }
        public string patientno { get; set; }
        public string ATL { get; set; }
        public string NTN { get; set; }
        public string CNIC { get; set; }
        public string CNICNAME { get; set; }
    }
}