# PHARMA - Pharmacy Management System

Windows desktop app (.NET Framework 4.8) — Windows 7+

## Database: **PharmaZ**

1. Run SQL scripts in folder `Database/`
2. Full create script: `Create_PharmaZ.sql` (creates DB + all 100 tables)
3. Connection string in `PHARMA.UI/App.config` already points to **PharmaZ**

```xml
Driver={SQL Server};Server=localhost;Database=PharmaZ;Uid=sa;Pwd=sa;
```

## Build
GitHub Actions builds `PHARMA.exe` — download from Actions → Artifacts → PHARMA-Release

## Features
- MDI parent, keyboard-centric (F2/F5/Ctrl+S)
- Login + rights
- POS / Sale billing
- ODBC → SQL Server
