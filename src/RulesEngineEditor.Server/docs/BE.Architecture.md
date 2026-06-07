# Backend Architecture – ASP.NET Core Web API

## Purpose
Provide validation and execution services for workflows defined in Microsoft RulesEngine JSON schema.  
Technologies: ASP.NET Core 10, EF Core, Microsoft RulesEngine library, PostgreSQL.

---

## Current Project Structure
RulesEngineEditor.Server/
├── RulesEngineEditor.Server.slnx
├── RulesEngineEditor.Server.csproj
├── Program.cs
├── Controllers/
│   └── WeatherForecastController.cs
├── Properties/
├── docs/
│   ├── BE.AgentRoles.md
│   ├── BE.Architecture.md
│   ├── BE.DebugGuide.md
│   └── BE.SkillsAudit.md
├── appsettings.json
├── appsettings.Development.json
└── RulesEngineEditor.Server.http

---

## Planned Architecture (Future State)
WorkflowController.cs, SchemaController.cs, Services/, Models/, and Persistence/ directories will be added as workflow features are implemented.

| Component | Purpose | Status |
|-----------|---------|--------|
| `WeatherForecastController.cs` | Template/scaffold controller | Present (will be removed) |
| `WorkflowController.cs` | Endpoints for workflow validation & execution | Planned |
| `SchemaController.cs` | Schema metadata & validation utilities | Planned |
| `RulesEngineService.cs` | Wraps Microsoft RulesEngine library | Planned |
| `ValidationService.cs` | JSON schema checks & error reporting | Planned |
| `WorkflowDbContext.cs` | EF Core context for workflow persistence | Planned |

---

## Current Configuration
- **CORS**: Policy named `localhost` — allows `https://localhost` with any method/header
- **OpenAPI**: Mapped at `/openapi/v1.json` (development only)
- **Scalar UI**: Available at `/scalar/v1` (development only)
- **Database**: PostgreSQL configured via `Npgsql` (connection string in `appsettings.json`)
- **HTTPS**: Redirection enabled (`UseHttpsRedirection`)
- **Auth**: Authorization middleware present (no policies configured yet)

---

## API Endpoints (Current)
- `GET /` — Returns "RulesEngine Editor Web API." status message
- `GET /WeatherForecast` — Template endpoint (to be replaced)

---

## API Endpoints (Planned)
- `POST /api/workflows/validate` — Validates workflow JSON against RulesEngine schema
- `POST /api/workflows/execute` — Executes workflow (dry-run or real execution)
- `GET /api/workflows/{id}` — Retrieves stored workflow definition

---

## Integration Points
- **UI.React** (planned)  
  - Sends JSON payloads to `/validate` and `/execute`.  
  - Receives validation results and execution outcomes.

- **Shared Libraries**  
  - `RulesEngine` (at `../BE.Libraries/RulesEngine/`) ensures schema compliance across backend and CLI tools.

---

## Testing Strategy
- **Unit Tests (xUnit)**  
  - Validate `RulesEngineService` methods.  
  - Mock persistence layer for isolated tests.

- **Integration Tests (xUnit)**  
  - Use `WebApplicationFactory` to spin up in-memory API.  
  - Test endpoints with real JSON payloads.  
  - Use Testcontainers for PostgreSQL (not EF Core In-Memory).

- **API Contract Testing**  
  - Use Playwright's native `APIRequestContext` for headless API contract verification if needed.  
  - For exploratory/manual validation, use Scalar at `/scalar/v1`.

---

## Human-in-the-Loop
- Developers review EF Core migrations before applying.  
- Manually debug RulesEngine exceptions during development.  
- Approve agent-generated backend code via pull requests.  
- Validate API responses against expected schema manually when needed.

