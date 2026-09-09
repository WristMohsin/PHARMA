# PharmaZ Database Setup

## Database name
**PharmaZ** (renamed from PHARMA)

## Files
1. `01_Create_Database_PharmaZ.sql` - creates empty database
2. `Create_PharmaZ.sql` - full schema (100 tables) - download from release/artifacts or use the file provided by Grok

## How to run (SSMS)
1. Open SQL Server Management Studio
2. Connect to your SQL Server
3. Open `01_Create_Database_PharmaZ.sql` → Execute (F5)
4. Open full `Create_PharmaZ.sql` → Execute (F5)

## How to run (sqlcmd)
```bat
sqlcmd -S localhost -E -i 01_Create_Database_PharmaZ.sql
sqlcmd -S localhost -E -i Create_PharmaZ.sql
```

## App connection (App.config)
```
Driver={SQL Server};Server=localhost;Database=PharmaZ;Uid=sa;Pwd=YOUR_PASSWORD;
```

Or Windows auth:
```
Driver={SQL Server};Server=localhost;Database=PharmaZ;Trusted_Connection=Yes;
```
