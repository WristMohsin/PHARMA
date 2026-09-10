using System.Collections.Generic;

namespace PHARMA.UI.Helpers
{
    public class ModuleDef
    {
        public string Key { get; set; }
        public string MenuTitle { get; set; }
        public string MenuSubTitle { get; set; }
        public string OptionTitle { get; set; }
    }

    public static class ModuleCatalog
    {
        public static readonly IList<ModuleDef> All = new List<ModuleDef>
        {
            new ModuleDef { Key = "POS", MenuTitle = "Sale", MenuSubTitle = "Billing", OptionTitle = "POS / Billing" },
            new ModuleDef { Key = "SALE_HISTORY", MenuTitle = "Sale", MenuSubTitle = "History", OptionTitle = "Sale History" },
            new ModuleDef { Key = "SALE_RETURN", MenuTitle = "Sale", MenuSubTitle = "Return", OptionTitle = "Sale Return" },
            new ModuleDef { Key = "PURCHASE", MenuTitle = "Purchase", MenuSubTitle = "Entry", OptionTitle = "Purchase Entry" },
            new ModuleDef { Key = "PRODUCTS", MenuTitle = "Inventory", MenuSubTitle = "Stock", OptionTitle = "Products" },
            new ModuleDef { Key = "ACCOUNTS", MenuTitle = "Accounts", MenuSubTitle = "Parties", OptionTitle = "Parties / Accounts" },
            new ModuleDef { Key = "PAYMENT", MenuTitle = "Accounts", MenuSubTitle = "Payment", OptionTitle = "Payment / Receipt" },
            new ModuleDef { Key = "COMPANIES", MenuTitle = "Masters", MenuSubTitle = "Company", OptionTitle = "Companies" },
        };
    }
}
