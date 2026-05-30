# Backend Architecture – ASP.NET Core Web API

## Purpose
Provide validation and execution services for workflows defined in Microsoft RulesEngine JSON schema.  
Technologies: ASP.NET Core 10, EF Core, Microsoft RulesEngine library, SQL/PostgreSQL.

---

## Project Structure
BE.Api/
├── BE.Api.sln
├── BE.Api.csproj
├── Program.cs
├── Controllers/
│   ├── WorkflowController.cs
│   └── SchemaController.cs
├── Services/
│   ├── RulesEngineService.cs
│   └── ValidationService.cs
├── Models/
│   └── WorkflowModel.cs
├── Persistence/
│   └── WorkflowDbContext.cs
└── docs/
├── BE.AgentRoles.md
├── BE.Architecture.md
└── BE.DebugGuide.md


---

## Core Components
- **WorkflowController.cs**  
  Exposes endpoints for workflow validation and execution.

- **SchemaController.cs**  
  Provides schema metadata and validation utilities.

- **RulesEngineService.cs**  
  Wraps Microsoft RulesEngine library for execution and validation.

- **ValidationService.cs**  
  Performs JSON schema checks and error reporting.

- **WorkflowDbContext.cs**  
  EF Core context for persisting workflows and execution logs.

---

## API Endpoints
- `POST /api/workflows/validate`  
  Validates workflow JSON against RulesEngine schema.

- `POST /api/workflows/execute`  
  Executes workflow (dry‑run or real execution).

- `GET /api/workflows/{id}`  
  Retrieves stored workflow definition.

---

## Integration Points
- **UI.React**  
  - Sends JSON payloads to `/validate` and `/execute`.  
  - Receives validation results and execution outcomes.

- **Shared Libraries**  
  - `RulesEngine` ensures schema compliance across backend and CLI tools.

---

## Testing Strategy
- **Unit Tests (xUnit)**  
  - Validate `RulesEngineService` methods.  
  - Mock persistence layer for isolated tests.

- **Integration Tests (xUnit)**  
  - Use `WebApplicationFactory` to spin up in‑memory API.  
  - Test endpoints with real JSON payloads.  
  - Validate DB persistence with test database.

- **Optional E2E (Playwright + Scalar)**  
  - Automate API endpoint testing via Scalar UI.  
  - Recommended for exploratory/manual validation, not required for CI/CD.

---

## Debugging Guide
- Run backend with `dotnet run`.  
- Use Visual Studio 2026 breakpoints in controllers and services.  
- Inspect logs in `WorkflowDbContext` for persistence issues.  
- Test endpoints with Swagger or Postman.  
- Validate schema alignment with Microsoft RulesEngine NuGet.

---

## Human‑in‑the‑Loop
- Developers review EF Core migrations before applying.  
- Manually debug RulesEngine exceptions in `RulesEngineService.cs`.  
- Approve agent‑generated backend code via pull requests.  
- Validate API responses against expected schema manually when needed.

