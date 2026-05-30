# Testing Strategy – RulesEngine Workflow Editor

## Framework Choice
- **Backend (ASP.NET Core Web API)**: Use **xUnit** as the sole testing framework.  
  - Rationale: De‑facto standard for .NET Core projects, integrates natively with modern IDE Test Explorers, and handles both unit and integration layers.  
  - Constraint: Avoid mixing with NUnit or MSTest to reduce runner friction and configuration overhead.

- **Frontend (React UI via Vite)**: 
  - Use **Vitest + React Testing Library (RTL)** for isolated component, panel form, and state logic.
  - Use **Playwright** for End‑to‑End (E2E) workflow automation.  
  - Rationale: ReactFlow uses standard React DOM elements under the hood. This makes Playwright locators highly reliable for handling drag-and-drop, bounding-box clicks, and UI assertions.

## Backend Testing Layers
- **Unit Tests (xUnit)**  
  - Validate core `RulesEngineService` logic, nested evaluation branches, and strict JSON schema builders.  
  - Mock third-party service dependencies using Moq or NSubstitute.
  - *Constraint*: Avoid EF Core In-Memory database providers as they do not mirror relational PostgreSQL behavior (e.g., constraints and JSONB behavior). Use unit-level mocking or SQLite in-memory only if absolutely necessary.

- **Integration Tests (xUnit + WebApplicationFactory)**  
  - Spin up the API host in-memory using `WebApplicationFactory<Program>`.  
  - Test real HTTP responses on endpoints (`/validate`, `/execute`) using valid and invalid JSON payloads.  
  - *Database Isolation*: Use **Testcontainers for .NET** to dynamically spin up an ephemeral PostgreSQL Docker container per test run. This guarantees schema correctness (UUIDs, JSONB) while avoiding test contamination.

## Frontend Testing Layers
- **Component & Logic Unit Tests (Vitest + RTL)**  
  - Validate individual ReactFlow custom nodes, sidebar stencils, validation banners, and property forms.
  - *Critical Coverage*: Extensively unit test the pure JavaScript data-transformer utility functions that map raw ReactFlow state objects (nodes/edges) into the backend `RulesEngine` JSON payload structure.

- **E2E Tests (Playwright)**  
  - Simulate the full human-in-the-loop user path: dragging a rule stencil onto the ReactFlow canvas pane, connecting nodes via handles, and modifying data parameters in side panels.
  - Click the canvas "Save/Validate" trigger to assert that structural payloads flow correctly to the backend and errors surface appropriately in the UI.

## Recommended Balance (The Testing Pyramid)
1. **Unit (xUnit & Vitest)**: High Volume. Fast execution feedback loops for developers. Catches logical rule bugs and state mapping flaws instantly.
2. **Integration (xUnit + Testcontainers)**: Medium Volume. Assures database integrity, API serialization, and HTTP status codes match system specs.
3. **E2E (Playwright)**: Low Volume. Focused exclusively on high-priority critical user journeys (e.g., Create Rule -> Connect Logic -> Validate Schema -> Deploy Live). 
*Note: Drop the Playwright + Scalar UI pattern. Rely on native headless `APIRequestContext` inside Playwright if backend contract verification is required during UI flows.*

## CI/CD Integration
- **Commit / Pull Request Pipeline**: Run all xUnit unit/integration tests and Vitest frontend tests. Block merges on any failure.
- **Nightly / Release Pipeline**: Deploy the application into a staging environment and execute the Playwright E2E automation suite to catch regression bugs or environmental anomalies.
