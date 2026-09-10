using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Models;
using PHARMA.Services;

namespace PHARMA.UI.Helpers
{
    /// <summary>
    /// Primary navigation from MenuName + UserRights.
    /// Fallback: ModuleCatalog filtered by rights (never exposes all modules to non-admin).
    /// </summary>
    public static class MenuBuilder
    {
        public static void Build(MenuStrip menuStrip, AuthService auth, Action<string> openModule)
        {
            if (menuStrip == null || auth == null || openModule == null) return;

            List<MenuName> items;
            try
            {
                items = ResolveMenuItems(auth);
            }
            catch (Exception ex)
            {
                Trace.WriteLine("MenuBuilder.Build failed: " + ex);
                return;
            }

            if (items == null || items.Count == 0) return;

            items = Deduplicate(items);

            var groups = items
                .GroupBy(m => string.IsNullOrEmpty(m.MenuTitle) ? "Other" : m.MenuTitle.Trim())
                .OrderBy(g => g.Min(x => x.ButtonName))
                .ThenBy(g => g.Key, StringComparer.OrdinalIgnoreCase);

            foreach (var g in groups)
            {
                var top = new ToolStripMenuItem(g.Key);
                foreach (var item in g
                    .OrderBy(x => x.ButtonName)
                    .ThenBy(x => x.MenuSubTitle ?? "", StringComparer.OrdinalIgnoreCase)
                    .ThenBy(x => x.OptionTitle ?? "", StringComparer.OrdinalIgnoreCase))
                {
                    string caption = !string.IsNullOrEmpty(item.OptionTitle)
                        ? item.OptionTitle
                        : item.MenuSubTitle;
                    if (string.IsNullOrEmpty(caption)) continue;

                    string key = !string.IsNullOrEmpty(item.OptionVariable)
                        ? item.OptionVariable
                        : caption;

                    var mi = new ToolStripMenuItem(caption.Trim());
                    mi.Tag = key;
                    string captured = key;
                    mi.Click += (s, e) => openModule(captured);
                    top.DropDownItems.Add(mi);
                }
                if (top.DropDownItems.Count > 0)
                    menuStrip.Items.Add(top);
            }
        }

        private static List<MenuName> Deduplicate(List<MenuName> items)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var result = new List<MenuName>();
            foreach (var m in items)
            {
                string id = !string.IsNullOrEmpty(m.OptionVariable)
                    ? "V:" + m.OptionVariable.Trim()
                    : "T:" + (m.MenuTitle ?? "") + "|" + (m.OptionTitle ?? m.MenuSubTitle ?? "");
                if (!seen.Add(id)) continue;
                result.Add(m);
            }
            return result;
        }

        private static List<MenuName> ResolveMenuItems(AuthService auth)
        {
            List<MenuName> fromDb = new List<MenuName>();
            try
            {
                var q = auth.GetMenusForUser();
                if (q != null)
                    fromDb = q.ToList();
            }
            catch (Exception ex)
            {
                Trace.WriteLine("MenuBuilder: GetMenusForUser failed: " + ex.Message);
                fromDb = new List<MenuName>();
            }

            if (fromDb.Count > 0)
                return fromDb;

            bool isAdmin = auth.IsAdmin();
            var rights = AuthService.CurrentRights ?? new List<UserRights>();
            var result = new List<MenuName>();
            int order = 0;
            foreach (var m in ModuleCatalog.All)
            {
                if (isAdmin || HasRight(rights, m))
                {
                    result.Add(new MenuName
                    {
                        MenuTitle = m.MenuTitle,
                        MenuSubTitle = m.MenuSubTitle,
                        OptionTitle = m.OptionTitle,
                        OptionVariable = m.Key,
                        ButtonName = order++
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

        public static HashSet<string> GetAuthorizedModuleKeys(AuthService auth)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            try
            {
                foreach (var m in ResolveMenuItems(auth))
                {
                    if (!string.IsNullOrEmpty(m.OptionVariable))
                        keys.Add(m.OptionVariable.Trim());
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine("GetAuthorizedModuleKeys: " + ex.Message);
            }
            return keys;
        }
    }
}
