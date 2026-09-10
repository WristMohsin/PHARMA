USE PharmaZ;
GO

IF OBJECT_ID('dbo.MenuName','U') IS NOT NULL
BEGIN
    DELETE FROM dbo.MenuName;

    INSERT INTO dbo.MenuName (MenuTitle, MenuSubTitle, OptionTitle, OptionVariable, ButtonName) VALUES
    ('Sale', 'Billing', 'POS / Billing', 'POS', 0),
    ('Sale', 'History', 'Sale History', 'SALE_HISTORY', 0),
    ('Sale', 'Return', 'Sale Return', 'SALE_RETURN', 0),
    ('Purchase', 'Entry', 'Purchase Entry', 'PURCHASE', 0),
    ('Inventory', 'Stock', 'Products', 'PRODUCTS', 0),
    ('Accounts', 'Parties', 'Parties / Accounts', 'ACCOUNTS', 0),
    ('Accounts', 'Payment', 'Payment / Receipt', 'PAYMENT', 0),
    ('Masters', 'Company', 'Companies', 'COMPANIES', 0);
END
GO

IF OBJECT_ID('dbo.UserRights','U') IS NOT NULL
BEGIN
    DELETE FROM dbo.UserRights WHERE UserName = 'admin';

    INSERT INTO dbo.UserRights (UserName, MenuTitle, MenuSubTitle, OptionTitle, OptionVariable, YNO, ButtonName)
    SELECT 'admin', MenuTitle, MenuSubTitle, OptionTitle, OptionVariable, 'Y', ButtonName
    FROM dbo.MenuName;
END
GO

IF OBJECT_ID('dbo.UserData','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.UserData WHERE UserName = 'admin')
        INSERT INTO dbo.UserData (UserName, PassWord, SecurityLevel)
        VALUES ('admin', 'admin', 'Admin');
    ELSE
        UPDATE dbo.UserData SET SecurityLevel = 'Admin' WHERE UserName = 'admin';
END
GO
