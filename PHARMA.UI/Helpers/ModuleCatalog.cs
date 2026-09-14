using System.Collections.Generic;

namespace PHARMA.UI.Helpers
{
    public class ModuleDef
    {
        public string Key { get; set; }
        public string MenuTitle { get; set; }
        public string MenuSubTitle { get; set; }
        public string OptionTitle { get; set; }
        public int ButtonName { get; set; }
    }

    /// <summary>
    /// Catalog of ERP modules. Order = menu display order (Management → Accounts → Purchase → Sale → Reports).
    /// </summary>
    public static class ModuleCatalog
    {
        public static readonly IList<ModuleDef> All = new List<ModuleDef>
        {
            // Management (1–6)
            new ModuleDef { Key = "CHART_OF_ACCOUNTS", MenuTitle = "Management", MenuSubTitle = "Setup", OptionTitle = "Chart of Accounts", ButtonName = 1 },
            new ModuleDef { Key = "PRODUCTS", MenuTitle = "Management", MenuSubTitle = "Setup", OptionTitle = "Products / New Item", ButtonName = 2 },
            new ModuleDef { Key = "USER_MGMT", MenuTitle = "Management", MenuSubTitle = "Security", OptionTitle = "User Management", ButtonName = 3 },
            new ModuleDef { Key = "USER_RIGHTS", MenuTitle = "Management", MenuSubTitle = "Security", OptionTitle = "User Rights", ButtonName = 4 },
            new ModuleDef { Key = "SHIFT_CLOSE", MenuTitle = "Management", MenuSubTitle = "Operations", OptionTitle = "Shift Close", ButtonName = 5 },
            new ModuleDef { Key = "APP_CONFIG", MenuTitle = "Management", MenuSubTitle = "Setup", OptionTitle = "Application Configuration", ButtonName = 6 },

            // Accounts (10–16)
            new ModuleDef { Key = "VOUCHER_CASH_IN", MenuTitle = "Accounts", MenuSubTitle = "Vouchers", OptionTitle = "Cash In Voucher", ButtonName = 10 },
            new ModuleDef { Key = "VOUCHER_CASH_OUT", MenuTitle = "Accounts", MenuSubTitle = "Vouchers", OptionTitle = "Cash Out Voucher", ButtonName = 11 },
            new ModuleDef { Key = "VOUCHER_EXPENSE", MenuTitle = "Accounts", MenuSubTitle = "Vouchers", OptionTitle = "Expense Voucher", ButtonName = 12 },
            new ModuleDef { Key = "CREDIT_NOTE", MenuTitle = "Accounts", MenuSubTitle = "Notes", OptionTitle = "Credit Note", ButtonName = 13 },
            new ModuleDef { Key = "DEBIT_NOTE", MenuTitle = "Accounts", MenuSubTitle = "Notes", OptionTitle = "Debit Note", ButtonName = 14 },
            new ModuleDef { Key = "ACCOUNTS", MenuTitle = "Accounts", MenuSubTitle = "Parties", OptionTitle = "Accounts", ButtonName = 15 },
            new ModuleDef { Key = "PAYMENT", MenuTitle = "Accounts", MenuSubTitle = "Parties", OptionTitle = "Payment / Receipt", ButtonName = 16 },

            // Purchase (20–23)
            new ModuleDef { Key = "PURCHASE", MenuTitle = "Purchase", MenuSubTitle = "Entry", OptionTitle = "Purchase", ButtonName = 20 },
            new ModuleDef { Key = "PURCHASE_MODIFY", MenuTitle = "Purchase", MenuSubTitle = "Entry", OptionTitle = "Purchase Modify", ButtonName = 21 },
            new ModuleDef { Key = "PURCHASE_RETURN", MenuTitle = "Purchase", MenuSubTitle = "Return", OptionTitle = "Purchase Return", ButtonName = 22 },
            new ModuleDef { Key = "PURCHASE_RETURN_MODIFY", MenuTitle = "Purchase", MenuSubTitle = "Return", OptionTitle = "Purchase Return Modify", ButtonName = 23 },

            // Sale (30–42)
            new ModuleDef { Key = "POS", MenuTitle = "Sale", MenuSubTitle = "Invoice", OptionTitle = "POS / Sale", ButtonName = 30 },
            new ModuleDef { Key = "SALE_MODIFY", MenuTitle = "Sale", MenuSubTitle = "Invoice", OptionTitle = "Sale Modify", ButtonName = 31 },
            new ModuleDef { Key = "SALE_WHOLESALE", MenuTitle = "Sale", MenuSubTitle = "Invoice", OptionTitle = "Wholesale Sale", ButtonName = 32 },
            new ModuleDef { Key = "SALE_WHOLESALE_MODIFY", MenuTitle = "Sale", MenuSubTitle = "Invoice", OptionTitle = "Wholesale Sale Modify", ButtonName = 33 },
            new ModuleDef { Key = "SALE_RETURN", MenuTitle = "Sale", MenuSubTitle = "Return", OptionTitle = "Sale Return", ButtonName = 34 },
            new ModuleDef { Key = "SALE_RETURN_MODIFY", MenuTitle = "Sale", MenuSubTitle = "Return", OptionTitle = "Sale Return Modify", ButtonName = 35 },
            new ModuleDef { Key = "SALE_RETURN_OPEN", MenuTitle = "Sale", MenuSubTitle = "Return", OptionTitle = "Sale Return Open", ButtonName = 36 },
            new ModuleDef { Key = "SALE_RETURN_OPEN_MODIFY", MenuTitle = "Sale", MenuSubTitle = "Return", OptionTitle = "Sale Return Open Modify", ButtonName = 37 },
            new ModuleDef { Key = "PRINT_THERMAL", MenuTitle = "Sale", MenuSubTitle = "Print", OptionTitle = "Print Thermal", ButtonName = 38 },
            new ModuleDef { Key = "PRINT_WHOLESALE", MenuTitle = "Sale", MenuSubTitle = "Print", OptionTitle = "Print Wholesale", ButtonName = 39 },
            new ModuleDef { Key = "SALE_DUMMY", MenuTitle = "Sale", MenuSubTitle = "Invoice", OptionTitle = "Dummy Sale", ButtonName = 40 },
            new ModuleDef { Key = "PRINT_DUMMY", MenuTitle = "Sale", MenuSubTitle = "Print", OptionTitle = "Print Dummy", ButtonName = 41 },
            new ModuleDef { Key = "SALE_HISTORY", MenuTitle = "Sale", MenuSubTitle = "History", OptionTitle = "Sale History", ButtonName = 42 },

            // Reports (50–56)
            new ModuleDef { Key = "RPT_COA", MenuTitle = "Reports", MenuSubTitle = "List", OptionTitle = "Chart of Accounts Report", ButtonName = 50 },
            new ModuleDef { Key = "RPT_PRODUCTS", MenuTitle = "Reports", MenuSubTitle = "List", OptionTitle = "Products Report", ButtonName = 51 },
            new ModuleDef { Key = "RPT_COMPANIES", MenuTitle = "Reports", MenuSubTitle = "List", OptionTitle = "Companies Report", ButtonName = 52 },
            new ModuleDef { Key = "RPT_CUSTOMERS", MenuTitle = "Reports", MenuSubTitle = "List", OptionTitle = "Customers Report", ButtonName = 53 },
            new ModuleDef { Key = "RPT_ACCOUNT", MenuTitle = "Reports", MenuSubTitle = "Financial", OptionTitle = "Account Report", ButtonName = 54 },
            new ModuleDef { Key = "RPT_PROFIT", MenuTitle = "Reports", MenuSubTitle = "Financial", OptionTitle = "Profit Report", ButtonName = 55 },
            new ModuleDef { Key = "RPT_SHIFT", MenuTitle = "Reports", MenuSubTitle = "Operations", OptionTitle = "Shift Report", ButtonName = 56 },
        };
    }
}
