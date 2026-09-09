-- =============================================
-- Step 1: Create database PharmaZ only
-- =============================================

IF DB_ID(N'PharmaZ') IS NULL
BEGIN
    CREATE DATABASE [PharmaZ];
    PRINT 'Database PharmaZ created.';
END
ELSE
BEGIN
    PRINT 'Database PharmaZ already exists.';
END
GO

USE [PharmaZ];
GO

PRINT 'Now using database PharmaZ. Run Create_PharmaZ.sql next (full tables).';
GO
