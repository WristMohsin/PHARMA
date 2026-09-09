namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.UserRights
    /// </summary>
    public class UserRights
    {
        public string UserName { get; set; }
        public string MenuTitle { get; set; }
        public string MenuSubTitle { get; set; }
        public string OptionTitle { get; set; }
        public string OptionVariable { get; set; }
        public string YNO { get; set; }
        public int ButtonName { get; set; }
    }
}