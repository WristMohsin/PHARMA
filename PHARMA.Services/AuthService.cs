using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            ClearSession();

            if (string.IsNullOrEmpty(username) || password == null)
                return false;

            UserData user;
            try
            {
                user = _repo.ValidateUser(username.Trim(), password);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthService.Login ValidateUser failed: " + ex.Message);
                ClearSession();
                throw;
            }

            if (user == null || string.IsNullOrEmpty(user.UserName))
            {
                ClearSession();
                return false;
            }

            CurrentUser = user;

            try
            {
                string rightsKey = CurrentUser.RightsKey;
                // Successful query may return zero rows — that is valid (restricted user)
                CurrentRights = _repo.GetUserRights(rightsKey) ?? new List<UserRights>();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthService.Login GetUserRights failed: " + ex.Message);
                ClearSession();
                throw;
            }

            return true;
        }

        public void Logout()
        {
            ClearSession();
        }

        private static void ClearSession()
        {
            CurrentUser = null;
            CurrentRights = new List<UserRights>();
        }

        public bool IsAdmin()
        {
            if (CurrentUser == null) return false;
            if (string.IsNullOrEmpty(CurrentUser.SecurityLevel)) return false;
            return string.Equals(CurrentUser.SecurityLevel, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        public bool HasRight(string menuTitle, string optionTitle = null)
        {
            if (CurrentUser == null) return false;
            if (IsAdmin()) return true;

            var rights = CurrentRights ?? new List<UserRights>();
            return rights.Any(r =>
                IsAllowedFlag(r.YNO) &&
                string.Equals(r.MenuTitle, menuTitle, StringComparison.OrdinalIgnoreCase) &&
                (optionTitle == null
                    || string.Equals(r.OptionTitle, optionTitle, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(r.OptionVariable, optionTitle, StringComparison.OrdinalIgnoreCase)));
        }

        public bool HasModuleRight(string moduleKey)
        {
            if (CurrentUser == null) return false;
            if (IsAdmin()) return true;

            string key = NormalizeModuleKey(moduleKey);
            if (string.IsNullOrEmpty(key)) return false;

            var rights = CurrentRights;
            if (rights == null || rights.Count == 0)
                return false;

            if (rights.Any(r => IsAllowedFlag(r.YNO) &&
                string.Equals(r.OptionVariable, key, StringComparison.OrdinalIgnoreCase)))
                return true;

            string menuTitle = null;
            string optionTitle = null;
            if (TryGetCatalogTitles(key, out menuTitle, out optionTitle))
            {
                if (rights.Any(r => IsAllowedFlag(r.YNO) &&
                    string.Equals(r.MenuTitle, menuTitle, StringComparison.OrdinalIgnoreCase) &&
                    (string.IsNullOrEmpty(r.OptionTitle)
                     || string.Equals(r.OptionTitle, optionTitle, StringComparison.OrdinalIgnoreCase)
                     || string.Equals(r.OptionVariable, key, StringComparison.OrdinalIgnoreCase))))
                    return true;
            }

            if (rights.Any(r => IsAllowedFlag(r.YNO) &&
                string.Equals(r.OptionTitle, key, StringComparison.OrdinalIgnoreCase)))
                return true;

            return false;
        }

        public static string NormalizeModuleKey(string moduleKey)
        {
            if (string.IsNullOrEmpty(moduleKey)) return string.Empty;

            string raw = moduleKey.Trim();
            string k = raw.ToUpperInvariant();

            if (k == "POS" || k == "SALE_HISTORY" || k == "SALE_RETURN" || k == "PURCHASE"
                || k == "PRODUCTS" || k == "ACCOUNTS" || k == "PAYMENT" || k == "COMPANIES")
                return k;

            if (string.Equals(raw, "POS / Billing", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "POS/Billing", StringComparison.OrdinalIgnoreCase)
                || k == "BILLING")
                return "POS";

            if (string.Equals(raw, "Sale History", StringComparison.OrdinalIgnoreCase)
                || k == "HISTORY" || k == "SALEHISTORY")
                return "SALE_HISTORY";

            if (string.Equals(raw, "Sale Return", StringComparison.OrdinalIgnoreCase)
                || k == "RETURN" || k == "SALERETURN")
                return "SALE_RETURN";

            if (string.Equals(raw, "Purchase Entry", StringComparison.OrdinalIgnoreCase)
                || k == "PUR" || k == "PURCHASE_ENTRY")
                return "PURCHASE";

            if (string.Equals(raw, "Products", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "Products / Stock", StringComparison.OrdinalIgnoreCase)
                || k == "STOCK" || k == "PRODUCT" || k == "INVENTORY")
                return "PRODUCTS";

            if (string.Equals(raw, "Parties / Accounts", StringComparison.OrdinalIgnoreCase)
                || string.Equals(raw, "Parties", StringComparison.OrdinalIgnoreCase)
                || k == "PARTY" || k == "PARTIES")
                return "ACCOUNTS";

            if (string.Equals(raw, "Payment / Receipt", StringComparison.OrdinalIgnoreCase)
                || k == "RECEIPT" || k == "PAYMENT_RECEIPT")
                return "PAYMENT";

            if (string.Equals(raw, "Companies", StringComparison.OrdinalIgnoreCase)
                || k == "COMPANY")
                return "COMPANIES";

            k = k.Replace(" ", "_").Replace("/", "_").Replace("-", "_");
            if (k == "POS_BILLING" || k == "SALE_BILLING") return "POS";
            if (k == "SALE_HISTORY" || k == "SALE_RETURN" || k == "PURCHASE"
                || k == "PRODUCTS" || k == "ACCOUNTS" || k == "PAYMENT" || k == "COMPANIES" || k == "POS")
                return k;

            return k;
        }

        private static bool TryGetCatalogTitles(string key, out string menuTitle, out string optionTitle)
        {
            menuTitle = null;
            optionTitle = null;
            switch (key)
            {
                case "POS": menuTitle = "Sale"; optionTitle = "POS / Billing"; return true;
                case "SALE_HISTORY": menuTitle = "Sale"; optionTitle = "Sale History"; return true;
                case "SALE_RETURN": menuTitle = "Sale"; optionTitle = "Sale Return"; return true;
                case "PURCHASE": menuTitle = "Purchase"; optionTitle = "Purchase Entry"; return true;
                case "PRODUCTS": menuTitle = "Inventory"; optionTitle = "Products"; return true;
                case "ACCOUNTS": menuTitle = "Accounts"; optionTitle = "Parties / Accounts"; return true;
                case "PAYMENT": menuTitle = "Accounts"; optionTitle = "Payment / Receipt"; return true;
                case "COMPANIES": menuTitle = "Masters"; optionTitle = "Companies"; return true;
                default: return false;
            }
        }

        private static bool IsAllowedFlag(string yno)
        {
            return yno == "Y" || yno == "1" || string.Equals(yno, "Yes", StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<MenuName> GetMenusForUser()
        {
            List<MenuName> all;
            try
            {
                all = _repo.GetAllMenus() ?? new List<MenuName>();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("AuthService.GetMenusForUser failed: " + ex.Message);
                return Enumerable.Empty<MenuName>();
            }

            if (all.Count == 0)
                return Enumerable.Empty<MenuName>();

            if (IsAdmin())
                return all;

            var allowedVars = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var allowedPairs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in CurrentRights ?? new List<UserRights>())
            {
                if (!IsAllowedFlag(r.YNO)) continue;
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
