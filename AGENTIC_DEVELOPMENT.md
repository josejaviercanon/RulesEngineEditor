# Agentic Development Guide

This repository uses agent-assisted workflows for OpenSpec implementation, validation, and database migration operations.

## UI Component Library Rule

For all UI work in `src/RulesEngine.UI`, contributors and agents MUST use the Blazorise UI component library.

- Use Blazorise components and patterns (`Form`, `Field`, `TextInput`, `Select`, `Modal`, `DataGrid`, `Button`, etc.) for new UI features.
- Use the configured providers in `src/RulesEngine.UI/Program.cs` (`Blazorise`, `Blazorise.Tailwind`, `Blazorise.Icons.Lucide`).
- Do not introduce new Radzen UI dependencies or new raw HTML form/grid/modal implementations for app features when an equivalent Blazorise component exists.
- When touching existing UI, prefer converging the edited surface area to Blazorise components.
- For component APIs and usage patterns, consult local Blazorise sources and demos first:
  - `./Blazorise/Source`
  - `./Blazorise/Demos`

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

## Workflow JSON Rollout and Rollback

Deployment sequence for `WorkflowJson` / `RuleJson` persistence:

1. Apply migrations first so the `workflows.WorkflowJson` column exists.
2. Deploy API/app code that reads `WorkflowJson` with fallback to legacy workflow JSON payloads.
3. Let startup backfill populate missing `WorkflowJson` and ensure each `rules.RuleJson` contains plain-text `Expression`.
4. Validate with API smoke tests: create, update, list/get, execute workflow.

Rollback sequence:

1. Roll back app binaries first (older code keeps working because legacy payload fallback is maintained).
2. Keep the new DB column in place during app rollback to avoid data loss.
3. Only drop `WorkflowJson` via migration rollback if you are also rolling back to a schema baseline that never references it.