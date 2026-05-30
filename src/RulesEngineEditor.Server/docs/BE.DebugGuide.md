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
- Inspect logs in `WorkflowDbContext` for persistence issues.
- Test endpoints with Swagger or Postman.

## Human Intervention
- Developers review EF Core schema changes.
- Validate API responses manually.
- Approve agent‑generated backend code via pull requests.
