# Backend Agent Roles – ASP.NET Core API

## Overview
The backend project validates and executes workflows using Microsoft RulesEngine.
Technologies: ASP.NET Core 10, EF Core, RulesEngine NuGet.

## Agent Responsibilities
- **Validation Agent**: Ensure incoming JSON matches RulesEngine schema.
- **Execution Agent**: Run dry‑runs or real workflow execution.
- **Persistence Agent**: Store workflows in SQL/PostgreSQL.
- **API Contract Agent**: Maintain REST endpoints (`/validate`, `/execute`).

## Human Revision Points
- Debug RulesEngine exceptions in `RulesEngineService.cs`.
- Review EF Core migrations for schema changes.
- Validate API responses with Postman/Swagger.

## Debug Guide
- Run solution with `dotnet run`.
- Use Visual Studio 2026 breakpoints in controllers/services.
- Inspect logs in `WorkflowDbContext` for persistence issues.
