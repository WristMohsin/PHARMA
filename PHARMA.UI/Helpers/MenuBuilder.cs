using System;
using System.Linq;
using System.Windows.Forms;
using PHARMA.Services;
using PHARMA.Models;

namespace PHARMA.UI.Helpers
{
    public static class MenuBuilder
    {
        public static void Build(MenuStrip menuStrip, AuthService auth, Action<string> openModule)
        {
            if (menuStrip == null || auth == null) return;

            System.Collections.Generic.IEnumerable<MenuName> menus = null;
            try
            {
                menus = auth.GetMenusForUser();
            }
            catch
            {
                return; // empty DB / no MenuName rows
            }

            if (menus == null) return;
            var list = menus.ToList();
            if (list.Count == 0) return;

            var groups = list.GroupBy(m => m.MenuTitle ?? "Other").OrderBy(g => g.Key);

            foreach (var g in groups)
            {
                // skip if already have same top menu from hardcoded
                bool exists = false;
                foreach (ToolStripItem existing in menuStrip.Items)
                {
                    if (string.Equals(existing.Text.Replace("&", ""), g.Key, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists) continue;

                var top = new ToolStripMenuItem(g.Key);
                foreach (var item in g.OrderBy(x => x.MenuSubTitle).ThenBy(x => x.OptionTitle))
                {
                    var caption = string.IsNullOrEmpty(item.OptionTitle) ? item.MenuSubTitle : item.OptionTitle;
                    if (string.IsNullOrEmpty(caption)) continue;

                    var mi = new ToolStripMenuItem(caption);
                    var key = item.OptionVariable ?? item.OptionTitle ?? item.MenuSubTitle;
                    mi.Tag = key;
                    mi.Click += (s, e) => openModule(key);
                    top.DropDownItems.Add(mi);
                }
                if (top.DropDownItems.Count > 0)
                    menuStrip.Items.Add(top);
            }
        }
    }
}
