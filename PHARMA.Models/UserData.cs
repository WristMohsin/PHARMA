namespace PHARMA.Models
{
    /// <summary>
    /// Authenticated user identity. Primary source: dbo.UserData.
    /// UserName is the rights key (UserRights.UserName).
    /// SecurityLevel drives Admin detection — never inferred from username text.
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

        public string DisplayName { get; set; }
        public string Company { get; set; }
        public string Grcd { get; set; }

        public string RightsKey
        {
            get { return UserName; }
        }

        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(DisplayName)) return DisplayName;
            return UserName ?? string.Empty;
        }
    }
}
