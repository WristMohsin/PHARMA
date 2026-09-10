-- Additive only: secure password storage (Change #4)
-- UserData.PassWord is nvarchar(20) — too small for PBKDF2 hash.
-- usertable.Password is varchar(50) — too small for full PBKDF2 hash.
USE PharmaZ;
GO

IF OBJECT_ID('dbo.UserData','U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.UserData', 'PasswordHash') IS NULL
    BEGIN
        ALTER TABLE dbo.UserData ADD PasswordHash NVARCHAR(200) NULL;
        PRINT 'Added UserData.PasswordHash NVARCHAR(200) NULL';
    END
    ELSE
        PRINT 'UserData.PasswordHash already exists';
END
GO

IF OBJECT_ID('dbo.usertable','U') IS NOT NULL
BEGIN
    IF COL_LENGTH('dbo.usertable', 'PasswordHash') IS NULL
    BEGIN
        ALTER TABLE dbo.usertable ADD PasswordHash VARCHAR(200) NULL;
        PRINT 'Added usertable.PasswordHash VARCHAR(200) NULL';
    END
    ELSE
        PRINT 'usertable.PasswordHash already exists';
END
GO
