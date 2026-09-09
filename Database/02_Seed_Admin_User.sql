USE PharmaZ;
GO
IF NOT EXISTS (SELECT 1 FROM UserData WHERE UserName = 'admin')
  INSERT INTO UserData (UserName, PassWord, SecurityLevel, Openrate, OpenDisc, OpenBonus, CheckCostRate, F3, F8, BarCodeMode, AdminDiscLevel, SupAC)
  VALUES ('admin', 'admin', 'Admin', 'Y', 'Y', 'Y', 'N', 'Y', 'Y', 'Y', 'Y', 'Y');
GO
IF NOT EXISTS (SELECT 1 FROM usertable WHERE Username = 'admin')
  INSERT INTO usertable (Username, Password, Company, Grcd) VALUES ('admin', 'admin', '01', '1');
GO
PRINT 'Admin user ready: admin / admin';
GO
