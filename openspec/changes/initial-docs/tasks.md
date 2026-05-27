## 1. Solution Scaffold

- [ ] 1.1 dotnet new sln RulesEngineWorkflowEditor
- [ ] 1.2 dotnet new classlib -n RulesEngine.Core
- [ ] 1.3 dotnet new classlib -n RulesEngine.Editor.Shared
- [ ] 1.4 dotnet new classlib -n RulesEngine.Infrastructure
- [ ] 1.5 dotnet new classlib -n RulesEngine.Application
- [ ] 1.6 dotnet new webapi -n RulesEngine.API
- [ ] 1.7 dotnet new blazorwasm -n RulesEngine.UI
- [ ] 1.8 dotnet new xunit -n RulesEngine.Tests
- [ ] 1.9 Add all projects to the solution and verify reference graph direction

## 2. Architecture Wiring

- [ ] 2.1 Add project references to enforce Core <- Application <- API/UI flow with Infrastructure behind interfaces
- [ ] 2.2 Add shared-editor contracts and helper abstractions
- [ ] 2.3 Add dependency injection registration extension methods per layer
- [ ] 2.4 Add baseline DTOs, commands, and MediatR handler shells

## 3. Workflow Lifecycle API

- [ ] 3.1 Add Minimal API route group for /api/workflows
- [ ] 3.2 Implement GET list and GET by id endpoints
- [ ] 3.3 Implement POST create endpoint with validation response contract
- [ ] 3.4 Implement PUT update endpoint with schema validation gate
- [ ] 3.5 Implement DELETE endpoint with proper HTTP semantics

## 4. Validation and Execution

- [ ] 4.1 Implement schema version resolution and validation service
- [ ] 4.2 Implement RulesEngine workflow registration wrapper in core
- [ ] 4.3 Implement dry-run execution path without persistence side effects
- [ ] 4.4 Implement real execution path with execution state persistence
- [ ] 4.5 Add structured error model for validation and execution failures

## 5. Persistence and Integrations

- [ ] 5.1 Implement EF Core DbContext and WorkflowEntity with JSON definition mapping
- [ ] 5.2 Implement repository interfaces and infrastructure implementations
- [ ] 5.3 Implement LogicFlow interop wrapper module (initializeCanvas, getGraphData, loadGraphData)
- [ ] 5.4 Implement /editor route in Blazor UI and wire Validate action to api
- [ ] 5.5 Add Radzen components for editor controls and feedback surfaces

## 6. Quality and Governance

- [ ] 6.1 Add unit tests for validation, execution orchestration, and repository behavior
- [ ] 6.2 Add integration tests for workflow CRUD and execution endpoints
- [ ] 6.3 Add JS interop contract tests for workflow canvas wrapper
- [ ] 6.4 Run openspec validate --strict and resolve all issues
- [ ] 6.5 Prepare archive readiness checklist for initial-docs change
