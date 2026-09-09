# PHARMA - Pharmacy / Clinic Management System

Windows desktop application for Pharmacy Management.

## Requirements
- Windows 7 / 8 / 10 / 11
- .NET Framework 4.8
- SQL Server (via ODBC)
- ODBC Driver for SQL Server installed

## Features
- MDI Parent interface
- Keyboard-centric (hotkeys, Tab navigation)
- Login with role-based rights
- Sale / POS (priority)
- Purchase, Inventory, Accounts, Masters
- Dashboard

## Connection
Uses **ODBC** connection string. Configure in `App.config`:

```xml
<connectionStrings>
  <add name="PHARMA" connectionString="Driver={ODBC Driver 17 for SQL Server};Server=YOUR_SERVER;Database=PHARMA;Uid=sa;Pwd=YOUR_PASSWORD;" providerName="System.Data.Odbc" />
</connectionStrings>
```

Or use SQL Server Native Client / older drivers for Windows 7 compatibility.

## Build
GitHub Actions produces `PHARMA.exe` + dependencies in artifacts.

## Modules
- Login & User Rights
- Sale / POS (barcode, hold, return)
- Purchase
- Inventory / Stock
- Accounts / Ledger
- Masters (CRUD)
- Attendance
- SMS
- Narcotics Register

Patient Token module is skipped as requested.
