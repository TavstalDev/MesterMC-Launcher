# Database (EF Core) — Quick Reference

This document shows common Entity Framework Core commands and tips for the ASP\.NET Core `Api` project. 

## Prerequisites
1. Have a local MySQL or MariaDb Server running and accessible.
2. Install .NET 9.0 SDK.
3. Ensure the `Api` project references `Microsoft.EntityFrameworkCore.Design` (for migrations).  
4. Install the EF Core CLI if needed:
```bash
dotnet tool install --global dotnet-ef
# or, if using a tool manifest in the repo:
dotnet tool restore
```
5. Create a database named `mmc` in your local SQL Server instance (or update the connection string in `appsettings.json` and `appsettings.Development.json` accordingly).

## Common Commands

### Add a Migration
```bash
dotnet ef migrations add <MigrationName> --project ./Api/Api.csproj --startup-project ./Api/Api.csproj
```

### Update the Database
```bash
dotnet ef database update --project ./Api/Api.csproj --startup-project ./Api/Api.csproj
```

### List Migrations
```bash
dotnet ef migrations list --project ./Api/Api.csproj --startup-project ./Api/Api.csproj
```

### Remove Last Migration
```bash
dotnet ef migrations remove --project ./Api/Api.csproj --startup-project ./Api/Api.csproj
```