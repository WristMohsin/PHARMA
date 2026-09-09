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
            menuStrip.Items.Clear();

            var menus = auth.GetMenusForUser().ToList();
            var groups = menus.GroupBy(m => m.MenuTitle).OrderBy(g => g.Key);

            foreach (var g in groups)
            {
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

            // Always add Exit
            var exit = new ToolStripMenuItem("Exit");
            exit.ShortcutKeys = Keys.Alt | Keys.F4;
            exit.Click += (s, e) => Application.Exit();
            menuStrip.Items.Add(exit);
        }
    }
}