USE PharmaZ;
GO

/* Phase A: Full ERP MenuName (37 modules) + admin UserRights only.
   Does NOT touch UserData passwords / PasswordHash.
   Safe to re-run: upserts MenuName by OptionVariable; refreshes admin rights from MenuName.
*/

IF OBJECT_ID('dbo.MenuName','U') IS NOT NULL
BEGIN
    /* Remove legacy top-level Inventory / Masters rows so they do not reappear */
    DELETE FROM dbo.MenuName
    WHERE MenuTitle IN (N'Inventory', N'Masters')
       OR OptionVariable IN (N'COMPANIES');

    ;WITH src AS (
        SELECT * FROM (VALUES
            (N'Management', N'Setup', N'Chart of Accounts', N'CHART_OF_ACCOUNTS', 1),
            (N'Management', N'Setup', N'Products / New Item', N'PRODUCTS', 2),
            (N'Management', N'Security', N'User Management', N'USER_MGMT', 3),
            (N'Management', N'Security', N'User Rights', N'USER_RIGHTS', 4),
            (N'Management', N'Operations', N'Shift Close', N'SHIFT_CLOSE', 5),
            (N'Management', N'Setup', N'Application Configuration', N'APP_CONFIG', 6),
            (N'Accounts', N'Vouchers', N'Cash In Voucher', N'VOUCHER_CASH_IN', 10),
            (N'Accounts', N'Vouchers', N'Cash Out Voucher', N'VOUCHER_CASH_OUT', 11),
            (N'Accounts', N'Vouchers', N'Expense Voucher', N'VOUCHER_EXPENSE', 12),
            (N'Accounts', N'Notes', N'Credit Note', N'CREDIT_NOTE', 13),
            (N'Accounts', N'Notes', N'Debit Note', N'DEBIT_NOTE', 14),
            (N'Accounts', N'Parties', N'Accounts', N'ACCOUNTS', 15),
            (N'Accounts', N'Parties', N'Payment / Receipt', N'PAYMENT', 16),
            (N'Purchase', N'Entry', N'Purchase', N'PURCHASE', 20),
            (N'Purchase', N'Entry', N'Purchase Modify', N'PURCHASE_MODIFY', 21),
            (N'Purchase', N'Return', N'Purchase Return', N'PURCHASE_RETURN', 22),
            (N'Purchase', N'Return', N'Purchase Return Modify', N'PURCHASE_RETURN_MODIFY', 23),
            (N'Sale', N'Invoice', N'POS / Sale', N'POS', 30),
            (N'Sale', N'Invoice', N'Sale Modify', N'SALE_MODIFY', 31),
            (N'Sale', N'Invoice', N'Wholesale Sale', N'SALE_WHOLESALE', 32),
            (N'Sale', N'Invoice', N'Wholesale Sale Modify', N'SALE_WHOLESALE_MODIFY', 33),
            (N'Sale', N'Return', N'Sale Return', N'SALE_RETURN', 34),
            (N'Sale', N'Return', N'Sale Return Modify', N'SALE_RETURN_MODIFY', 35),
            (N'Sale', N'Return', N'Sale Return Open', N'SALE_RETURN_OPEN', 36),
            (N'Sale', N'Return', N'Sale Return Open Modify', N'SALE_RETURN_OPEN_MODIFY', 37),
            (N'Sale', N'Print', N'Print Thermal', N'PRINT_THERMAL', 38),
            (N'Sale', N'Print', N'Print Wholesale', N'PRINT_WHOLESALE', 39),
            (N'Sale', N'Invoice', N'Dummy Sale', N'SALE_DUMMY', 40),
            (N'Sale', N'Print', N'Print Dummy', N'PRINT_DUMMY', 41),
            (N'Sale', N'History', N'Sale History', N'SALE_HISTORY', 42),
            (N'Reports', N'List', N'Chart of Accounts Report', N'RPT_COA', 50),
            (N'Reports', N'List', N'Products Report', N'RPT_PRODUCTS', 51),
            (N'Reports', N'List', N'Companies Report', N'RPT_COMPANIES', 52),
            (N'Reports', N'List', N'Customers Report', N'RPT_CUSTOMERS', 53),
            (N'Reports', N'Financial', N'Account Report', N'RPT_ACCOUNT', 54),
            (N'Reports', N'Financial', N'Profit Report', N'RPT_PROFIT', 55),
            (N'Reports', N'Operations', N'Shift Report', N'RPT_SHIFT', 56)
        ) AS v(MenuTitle, MenuSubTitle, OptionTitle, OptionVariable, ButtonName)
    )
    MERGE dbo.MenuName AS t
    USING src AS s
        ON t.OptionVariable = s.OptionVariable
    WHEN MATCHED THEN
        UPDATE SET
            t.MenuTitle = s.MenuTitle,
            t.MenuSubTitle = s.MenuSubTitle,
            t.OptionTitle = s.OptionTitle,
            t.ButtonName = s.ButtonName
    WHEN NOT MATCHED BY TARGET THEN
        INSERT (MenuTitle, MenuSubTitle, OptionTitle, OptionVariable, ButtonName)
        VALUES (s.MenuTitle, s.MenuSubTitle, s.OptionTitle, s.OptionVariable, s.ButtonName);

    DELETE FROM dbo.MenuName
    WHERE OptionVariable IS NOT NULL
      AND OptionVariable NOT IN (
        N'CHART_OF_ACCOUNTS', N'PRODUCTS', N'USER_MGMT', N'USER_RIGHTS', N'SHIFT_CLOSE', N'APP_CONFIG',
        N'VOUCHER_CASH_IN', N'VOUCHER_CASH_OUT', N'VOUCHER_EXPENSE', N'CREDIT_NOTE', N'DEBIT_NOTE', N'ACCOUNTS', N'PAYMENT',
        N'PURCHASE', N'PURCHASE_MODIFY', N'PURCHASE_RETURN', N'PURCHASE_RETURN_MODIFY',
        N'POS', N'SALE_MODIFY', N'SALE_WHOLESALE', N'SALE_WHOLESALE_MODIFY',
        N'SALE_RETURN', N'SALE_RETURN_MODIFY', N'SALE_RETURN_OPEN', N'SALE_RETURN_OPEN_MODIFY',
        N'PRINT_THERMAL', N'PRINT_WHOLESALE', N'SALE_DUMMY', N'PRINT_DUMMY', N'SALE_HISTORY',
        N'RPT_COA', N'RPT_PRODUCTS', N'RPT_COMPANIES', N'RPT_CUSTOMERS', N'RPT_ACCOUNT', N'RPT_PROFIT', N'RPT_SHIFT'
      );
END
GO

IF OBJECT_ID('dbo.UserRights','U') IS NOT NULL
BEGIN
    /* Refresh admin menu rights only — does not touch passwords */
    DELETE FROM dbo.UserRights WHERE UserName = N'admin';

    INSERT INTO dbo.UserRights (UserName, MenuTitle, MenuSubTitle, OptionTitle, OptionVariable, YNO, ButtonName)
    SELECT N'admin', MenuTitle, MenuSubTitle, OptionTitle, OptionVariable, N'Y', ButtonName
    FROM dbo.MenuName
    WHERE OptionVariable IS NOT NULL;
END
GO
