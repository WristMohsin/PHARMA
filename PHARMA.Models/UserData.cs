namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.UserData
    /// </summary>
    public class UserData
    {
        public string UserName { get; set; }
        public string PassWord { get; set; }
        public string SecurityLevel { get; set; }
        public string UserPrinter { get; set; }
        public string Openrate { get; set; }
        public string OpenDisc { get; set; }
        public string OpenBonus { get; set; }
        public string CheckCostRate { get; set; }
        public string F3 { get; set; }
        public string F8 { get; set; }
        public string BarCodeMode { get; set; }
        public string AdminDiscLevel { get; set; }
        public string SupAC { get; set; }
    }
}