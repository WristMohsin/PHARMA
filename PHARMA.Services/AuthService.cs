using System;
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
        public static List<UserRights> CurrentRights { get; private set; }

        static AuthService()
        {
            CurrentRights = new List<UserRights>();
        }

        public bool Login(string username, string password)
        {
            var user = _repo.ValidateUser(username, password);
            if (user == null) return false;

            CurrentUser = user;
            try
            {
                CurrentRights = _repo.GetUserRights(username) ?? new List<UserRights>();
            }
            catch
            {
                CurrentRights = new List<UserRights>();
            }
            return true;
        }

        public void Logout()
        {
            CurrentUser = null;
            CurrentRights = new List<UserRights>();
        }

        public bool IsAdmin()
        {
            return CurrentUser != null
                && CurrentUser.SecurityLevel != null
                && string.Equals(CurrentUser.SecurityLevel, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        public bool HasRight(string menuTitle, string optionTitle = null)
        {
            if (CurrentUser == null) return false;
            if (IsAdmin()) return true;

            return CurrentRights.Any(r =>
                (r.YNO == "Y" || r.YNO == "1") &&
                string.Equals(r.MenuTitle, menuTitle, StringComparison.OrdinalIgnoreCase) &&
                (optionTitle == null
                    || string.Equals(r.OptionTitle, optionTitle, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r.OptionVariable, optionTitle, StringComparison.OrdinalIgnoreCase)));
        }

        public bool HasModuleRight(string moduleKey)
        {
            if (CurrentUser == null) return false;
            if (IsAdmin()) return true;
            if (string.IsNullOrEmpty(moduleKey)) return false;

            return CurrentRights.Any(r =>
                (r.YNO == "Y" || r.YNO == "1") &&
                (string.Equals(r.OptionVariable, moduleKey, StringComparison.OrdinalIgnoreCase)
                 || string.Equals(r.OptionTitle, moduleKey, StringComparison.OrdinalIgnoreCase)
                 || (r.MenuTitle != null && r.MenuTitle.IndexOf(moduleKey, StringComparison.OrdinalIgnoreCase) >= 0)));
        }

        public IEnumerable<MenuName> GetMenusForUser()
        {
            List<MenuName> all;
            try
            {
                all = _repo.GetAllMenus() ?? new List<MenuName>();
            }
            catch
            {
                return Enumerable.Empty<MenuName>();
            }

            if (all.Count == 0)
                return Enumerable.Empty<MenuName>();

            if (IsAdmin())
                return all;

            var allowedVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allowedPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in CurrentRights)
            {
                if (r.YNO != "Y" && r.YNO != "1") continue;
                if (!string.IsNullOrEmpty(r.OptionVariable))
                    allowedVars.Add(r.OptionVariable);
                allowedPairs.Add((r.MenuTitle ?? "") + "|" + (r.OptionTitle ?? ""));
                allowedPairs.Add((r.MenuTitle ?? "") + "|");
            }

            return all.Where(m =>
                (!string.IsNullOrEmpty(m.OptionVariable) && allowedVars.Contains(m.OptionVariable))
                || allowedPairs.Contains((m.MenuTitle ?? "") + "|" + (m.OptionTitle ?? ""))
                || allowedPairs.Contains((m.MenuTitle ?? "") + "|"));
        }
    }
}
