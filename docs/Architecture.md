# Global Architecture – RulesEngine Workflow Editor Solution

## Purpose
Provide a unified architecture for the RulesEngine Workflow Editor, combining a React UI for workflow design with an ASP.NET Core 10 backend for validation and execution.  
This document defines the overall structure, integration points, and governance principles.

---

## Solution Structure

RulesEngineWorkflowEditor/
├── RulesEngineWorkflowEditor.sln
├── docs/
│   ├── Architecture.md
│   ├── Governance.md
│   ├── SprintPlaybook.md
│   ├── QA.md
├── src/
│   ├── UI.React/        # React workflow editor
│   ├── BE.Api/          # ASP.NET Core backend
│   ├── Shared/          # Shared libraries
│   ├── BE.tests/        # BE Test projects
│   ├── UI.tests/        # UI Test projects


---

## Layers
- **UI.React**  
  - Provides drag‑drop workflow editor.  
  - Exports JSON compliant with Microsoft RulesEngine schema.  
  - Integrates with backend via REST API.

- **BE.Api**  
  - Hosts validation and execution endpoints.  
  - Wraps Microsoft RulesEngine library for schema compliance.  
  - Persists workflows in SQL/PostgreSQL.

- **Shared Libraries**  
  - `RulesEngineWrapper` for schema validation and execution helpers.  
  - `WorkflowTests` for reusable test logic.

- **Tests**  
  - `UI.Tests` → Playwright E2E tests.  
  - `BE.Tests` → xUnit unit + integration tests.

---

## Integration Flow
1. **Workflow Creation (UI.React)**  
   - User designs workflow in ReactFlow.  
   - JSON exported via `SchemaExporter.ts`.

2. **Validation (BE.Api)**  
   - JSON sent to `/api/workflows/validate`.  
   - Backend validates against RulesEngine schema.  
   - Errors returned to UI for display.

3. **Execution (BE.Api)**  
   - JSON sent to `/api/workflows/execute`.  
   - Backend runs dry‑run or real execution.  
   - Results returned to UI.

4. **Persistence (BE.Api)**  
   - Workflows stored in SQL/PostgreSQL via EF Core.  
   - Retrieval via `/api/workflows/{id}`.

---

## Testing Strategy
- **Backend** → xUnit for unit + integration tests.  
- **Frontend** → React Testing Library (RTL) for isolated component, panel form, and state logic.
- **Frontend** → Playwright for E2E tests workflow automation.   
- **Optional** → Playwright + Scalar for exploratory API testing.  
- QA checklist maintained in `QA.md`.

---

## Governance Principles
- **Spec‑Driven Development**: All agent outputs validated against OpenSpec/Spec‑Kit.  
- **Human‑in‑the‑Loop**: Agents automate routine tasks; humans approve critical changes.  
- **Observability**: DevUI + OpenTelemetry for tracing and cost metrics.  
- **Auditability**: Approvals and revisions logged in DB and documented in `QA.md`.

---

## Deployment
- **CI/CD Pipeline**  
  - Build UI and backend separately.  
  - Publish unified artifact with validated workflows.  
  - Fail pipeline if tests or schema validation fail.

- **Hosting Options**  
  - Backend hosted on Azure App Service or Kubernetes.  
  - UI built with Vite and served via backend `wwwroot` or CDN.

---

## Human Revision Points
- UI: Review schema export logic and node rendering.  
- Backend: Debug RulesEngine exceptions and EF Core migrations.  
- Global: Approve agent‑generated changes via pull requests.  

