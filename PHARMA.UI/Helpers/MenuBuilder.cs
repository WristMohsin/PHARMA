using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;

namespace PHARMA.UI.Helpers
{
    public static class MenuBuilder
    {
        public static void Build(MenuStrip menuStrip, AuthService auth, Action<string> openModule)
        {
            if (menuStrip == null || auth == null || openModule == null) return;

            var items = ResolveMenuItems(auth);
            if (items.Count == 0) return;

            var groups = items
                .GroupBy(m => string.IsNullOrEmpty(m.MenuTitle) ? "Other" : m.MenuTitle)
                .OrderBy(g => g.Key);

            foreach (var g in groups)
            {
                var top = new ToolStripMenuItem(g.Key);
                foreach (var item in g.OrderBy(x => x.MenuSubTitle).ThenBy(x => x.OptionTitle))
                {
                    string caption = !string.IsNullOrEmpty(item.OptionTitle)
                        ? item.OptionTitle
                        : item.MenuSubTitle;
                    if (string.IsNullOrEmpty(caption)) continue;

                    string key = !string.IsNullOrEmpty(item.OptionVariable)
                        ? item.OptionVariable
                        : caption;

                    var mi = new ToolStripMenuItem(caption);
                    mi.Tag = key;
                    string captured = key;
                    mi.Click += (s, e) => openModule(captured);
                    top.DropDownItems.Add(mi);
                }
                if (top.DropDownItems.Count > 0)
                    menuStrip.Items.Add(top);
            }
        }

        private static List<MenuName> ResolveMenuItems(AuthService auth)
        {
            List<MenuName> fromDb = null;
            try
            {
                fromDb = auth.GetMenusForUser().ToList();
            }
            catch
            {
                fromDb = new List<MenuName>();
            }

            if (fromDb != null && fromDb.Count > 0)
                return fromDb;

            bool isAdmin = AuthService.CurrentUser != null
                && AuthService.CurrentUser.SecurityLevel != null
                && string.Equals(AuthService.CurrentUser.SecurityLevel, "Admin", StringComparison.OrdinalIgnoreCase);

            var rights = AuthService.CurrentRights ?? new List<UserRights>();
            var result = new List<MenuName>();
            foreach (var m in ModuleCatalog.All)
            {
                if (isAdmin || HasRight(rights, m))
                {
                    result.Add(new MenuName
                    {
                        MenuTitle = m.MenuTitle,
                        MenuSubTitle = m.MenuSubTitle,
                        OptionTitle = m.OptionTitle,
                        OptionVariable = m.Key
                    });
                }
            }
            return result;
        }

        private static bool HasRight(List<UserRights> rights, ModuleDef m)
        {
            if (rights == null || rights.Count == 0) return false;
            return rights.Any(r =>
                (r.YNO == "Y" || r.YNO == "1") &&
                (
                    string.Equals(r.OptionVariable, m.Key, StringComparison.OrdinalIgnoreCase) ||
                    (string.Equals(r.MenuTitle, m.MenuTitle, StringComparison.OrdinalIgnoreCase) &&
                     (string.IsNullOrEmpty(r.OptionTitle) ||
                      string.Equals(r.OptionTitle, m.OptionTitle, StringComparison.OrdinalIgnoreCase)))
                ));
        }
    }
}
