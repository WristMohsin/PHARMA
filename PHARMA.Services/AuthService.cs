using System.Collections.Generic;
using System.Linq;
using PHARMA.DataAccess.Repositories;
using PHARMA.Models;

namespace PHARMA.Services
{
    public class AuthService
    {
        private readonly AuthRepository _repo = new AuthRepository();

        public static UserData CurrentUser { get; private set; }
        public static List<UserRights> CurrentRights { get; private set; } = new List<UserRights>();

        public bool Login(string username, string password)
        {
            var user = _repo.ValidateUser(username, password);
            if (user == null) return false;

            CurrentUser = user;
            CurrentRights = _repo.GetUserRights(username).ToList();
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
            CurrentRights.Clear();
        }

        public bool HasRight(string menuTitle, string optionTitle = null)
        {
            if (CurrentUser == null) return false;
            // Admin bypass example
            if (string.Equals(CurrentUser.SecurityLevel, "Admin", System.StringComparison.OrdinalIgnoreCase))
                return true;

            return CurrentRights.Any(r =>
                string.Equals(r.MenuTitle, menuTitle, System.StringComparison.OrdinalIgnoreCase) &&
                (optionTitle == null || string.Equals(r.OptionTitle, optionTitle, System.StringComparison.OrdinalIgnoreCase)) &&
                (r.YNO == "Y" || r.YNO == "1"));
        }

        public IEnumerable<MenuName> GetMenusForUser()
        {
            var all = _repo.GetAllMenus();
            if (CurrentUser != null && string.Equals(CurrentUser.SecurityLevel, "Admin", System.StringComparison.OrdinalIgnoreCase))
                return all;

            var allowed = CurrentRights.Select(r => r.MenuTitle + "|" + r.OptionTitle).ToHashSet();
            return all.Where(m => allowed.Contains(m.MenuTitle + "|" + m.OptionTitle) || allowed.Contains(m.MenuTitle + "|"));
        }
    }
}