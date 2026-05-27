# Design: Core Engine & System Architecture

## Decisions

### Decision: Clean Architecture with 7 projects
- Core: Domain entities + RulesEngine wrapper + JSON schema validation
- Editor.Shared: Shared validation/test runner logic
- Infrastructure: EF Core DbContext + JSON column mapping + repositories
- Application: MediatR handlers + DTOs + service interfaces
- API: Minimal API endpoints (MapPost, MapGet, etc.)
- UI: Blazor WASM + Radzen Components + LogicFlow.js via IJSRuntime
- Tests: xUnit + FluentAssertions + TestServer integration tests

### Decision: JSON Column for Workflow Storage
- EF Core ToJson() mapping on WorkflowEntity.Definition
- Enables schema flexibility without migration churn for node types

### Decision: LogicFlow.js via Blazor JS Interop
- Wrap LogicFlow in a reusable WorkflowCanvas.razor.js module
- Expose initializeCanvas, getGraphData, loadGraphData to C#

## Data Flow
[Blazor SPA] --JSON--> [Minimal API] --> [Application Layer]
                                             |
                   [RulesEngine.Core] <-------|
                          |
                   [EF Core + JSON Column]
