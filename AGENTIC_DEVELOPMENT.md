# Agentic Development Guide

This repository uses agent-assisted workflows for OpenSpec implementation, validation, and database migration operations.

## Database Migration Rule

When running migrations from an agent workflow, always use the connection string from [src/RulesEngine.API/appsettings.Development.json](src/RulesEngine.API/appsettings.Development.json).

Current source of truth:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=GamificationFlow_DEV;Username=postgres;Password=postgres"
}
```

From this connection string, derive the target database name from `Database=...`.
For the current value, the target database is `GamificationFlow_DEV`.

## Agent Migration Command (PowerShell)

Run migrations with the following pattern:

```powershell
$settingsPath = "src/RulesEngine.API/appsettings.Development.json"
$settings = Get-Content $settingsPath -Raw | ConvertFrom-Json
$connection = $settings.ConnectionStrings.DefaultConnection

$databaseName = [regex]::Match($connection, 'Database=([^;]+)').Groups[1].Value
Write-Host "Applying migrations to database: $databaseName"

$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:RULES_ENGINE_EDITOR_CONNECTION = $connection

dotnet ef database update \
  --project src/RulesEngine.Infrastructure/RulesEngine.Infrastructure.csproj \
  --startup-project src/RulesEngine.API/RulesEngine.API.csproj \
  --context RulesEngineEditorDbContext
```

## Notes

- Keep `src/RulesEngine.API/appsettings.Development.json` as the default development source of truth for migration connection settings.
- Use `RULES_ENGINE_EDITOR_CONNECTION` only to pass the exact same value to design-time EF tooling.
- If the development connection string changes, migration automation must read it dynamically rather than hardcoding a database name.