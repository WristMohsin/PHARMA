namespace PHARMA.Models
{
    /// <summary>
    /// Maps to dbo.usertable. Password is varchar(50) legacy.
    /// PasswordHash is additive for secure storage when present.
    /// </summary>
    public class usertable
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public string Company { get; set; }
        public string Grcd { get; set; }
    }
}
