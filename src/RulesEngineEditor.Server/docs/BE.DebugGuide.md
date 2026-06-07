# Debug Guide – ASP.NET Core Backend

## Common Issues
1. **Validation Failures**
   - Symptom: Backend rejects valid workflow JSON.
   - Fix: Inspect `RulesEngineService.cs`; confirm schema alignment with Microsoft RulesEngine Library.

2. **Execution Errors**
   - Symptom: Workflow execution throws exceptions.
   - Fix: Debug `ExecuteWorkflow` method; add logging for rule evaluation.

3. **Persistence Problems**
   - Symptom: Workflows not saved or retrieved correctly.
   - Fix: Check EF Core migrations; validate `WorkflowDbContext` configuration.

## Debugging Steps
- Run backend with `dotnet run`.
- Use Visual Studio 2026 breakpoints in controllers and services.
- Inspect logs or database directly for persistence issues.
- Test endpoints with Scalar at `/scalar/v1` or Postman.
- Use `dotnet watch` for hot reload during development.

## .NET Hot Reload
- Use `dotnet watch` for hot reload during development.
- Changes to controllers, services, and views apply without restarting the process.
- Run: `dotnet watch run` from the project directory.
- Note: Some changes (e.g., new NuGet packages, static files) still require a restart.

## OpenAPI / Scalar UI
- Scalar UI is available in development mode at: `/scalar/v1`
- Enabled via `app.MapScalarApiReference()` in `Program.cs`
- Use Scalar to explore endpoints, send test requests, and view OpenAPI schema.
- The OpenAPI document is mapped at `/openapi/v1.json`.
- Note: Not Swagger UI — the project uses Scalar exclusively.

## OpenTelemetry Trace Inspection
- OpenTelemetry is **not yet configured** in the current project.
- When added, traces will include: incoming HTTP requests, database calls, and external service calls.
- Planned integrations:
  - Jaeger / Zipkin (self-hosted trace collectors)
  - Azure Application Insights (production)
  - OTEL-compatible logging tools

## PostgreSQL Connection Troubleshooting
- Connection string is configured in `appsettings.json` under `ConnectionStrings`.
- Common connection errors:
  - **Host not found**: Verify PostgreSQL server is running and accessible.
  - **Authentication failed**: Check username/password in connection string.
  - **Database not found**: Ensure the target database exists; run EF Core migrations.
- Test connection: `dotnet ef database update` or use `pg_isready` command-line tool.
- Npgsql-specific: For SSL issues, add `SslMode=Prefer` or `SslMode=Disable` to connection string.

## EF Core Migration Debugging
- Add migration: `dotnet ef migrations add <MigrationName>`
- Update database: `dotnet ef database update`
- Rollback: `dotnet ef database update <PreviousMigrationName>`
- Script migration: `dotnet ef migrations script`
- Common issues:
  - **Snapshot mismatch**: Delete the migration and re-add after synchronizing the model.
  - **Pending model changes**: Run `dotnet ef migrations has-pending-model-changes` to check.
  - **Design-time errors**: Ensure `Microsoft.EntityFrameworkCore.Design` is referenced.

## Human Intervention
- Developers review EF Core schema changes.
- Validate API responses manually.
- Approve agent‑generated backend code via pull requests.
