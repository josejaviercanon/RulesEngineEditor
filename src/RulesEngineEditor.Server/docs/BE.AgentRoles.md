# Backend Agent Roles – ASP.NET Core API

## Overview
The backend project validates and executes workflows using Microsoft RulesEngine.
Technologies: ASP.NET Core 10, EF Core, RulesEngine NuGet.

## Agent Responsibilities
- **Validation Agent**: Ensure incoming JSON matches RulesEngine schema.
- **Execution Agent**: Run dry‑runs or real workflow execution.
- **Persistence Agent**: Store workflows in PostgreSQL.
- **API Contract Agent**: Maintain REST endpoints (`/validate`, `/execute`).

## Agent Model Assignments

### Deepseek Models
- **Architect** → `deepseek/deepseek-v4-flash` (fast, cheap model for planning)
  - Permissions: `edit=allow`, `bash=deny` (writes spec files only)
- **Builder** → `deepseek/deepseek-v4-pro` (top-tier coding model)
  - Permissions: `edit=allow`, `bash=allow` (writes code, runs tests)
- **Reviewer** → `deepseek/deepseek-v4-pro` (high-reasoning model for audits)
  - Permissions: `edit=deny`, `bash=deny` (strictly read-only)

### Qwen Models
- **Architect** → `qwen/qwen-plus` (fast, cost-effective model for planning)
  - Permissions: `edit=allow`, `bash=deny` (writes spec files only)
- **Builder** → `qwen/qwen-max` (top-tier coding model)
  - Permissions: `edit=allow`, `bash=allow` (writes code, runs tests)
- **Reviewer** → `qwen/qwen-max` (high-reasoning model for audits)
  - Permissions: `edit=deny`, `bash=deny` (strictly read-only)

## Agent Skill Files
Detailed agent-specific context files are maintained in `.agents/skills/`:
- `.agents/skills/deepseek-architect.md` — architecture context for Deepseek planning agent
- `.agents/skills/deepseek-builder.md` — coding conventions for Deepseek implementation agent
- `.agents/skills/deepseek-reviewer.md` — review criteria for Deepseek auditing agent
- `.agents/skills/qwen-architect.md` — architecture context for Qwen planning agent
- `.agents/skills/qwen-builder.md` — coding conventions for Qwen implementation agent
- `.agents/skills/qwen-reviewer.md` — review criteria for Qwen auditing agent

## Human Revision Points
- Debug RulesEngine exceptions during development.
- Review EF Core migrations for schema changes.
- Validate API responses with Scalar at `/scalar/v1` or Postman.

## Debug Guide
- Run solution with `dotnet run` or `dotnet watch`.
- Use Visual Studio 2026 breakpoints in controllers/services.
- Use Scalar at `/scalar/v1` for API exploration in development mode.
