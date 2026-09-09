namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.Company
    /// </summary>
    public class Company
    {
        public string cmpcd { get; set; }
        public string cmpnm { get; set; }
        public string CompanyName { get; set; }
        public string CompanyAddress { get; set; }
        public string CompanyAddress1 { get; set; }
        public string Bank { get; set; }
        public string Branch { get; set; }
        public string Transport { get; set; }
        public int OldCode { get; set; }
        public int IntimationMonth { get; set; }
        public string mpost { get; set; }
        public int CompanyCode { get; set; }
    }
}