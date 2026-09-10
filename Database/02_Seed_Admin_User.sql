USE PharmaZ;
GO

IF OBJECT_ID('dbo.UserData','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.UserData WHERE UserName = 'admin')
    BEGIN
        INSERT INTO dbo.UserData (UserName, PassWord, SecurityLevel)
        VALUES ('admin', 'admin', 'Admin');
        PRINT 'Inserted UserData admin with SecurityLevel=Admin';
    END
    ELSE
    BEGIN
        UPDATE dbo.UserData
        SET SecurityLevel = 'Admin'
        WHERE UserName = 'admin'
          AND (SecurityLevel IS NULL OR LTRIM(RTRIM(SecurityLevel)) = '');
        PRINT 'Ensured UserData admin SecurityLevel=Admin when empty';
    END
END
GO

IF OBJECT_ID('dbo.usertable','U') IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM dbo.usertable WHERE Username = 'admin')
        INSERT INTO dbo.usertable (Username, Password) VALUES ('admin', 'admin');
END
GO
