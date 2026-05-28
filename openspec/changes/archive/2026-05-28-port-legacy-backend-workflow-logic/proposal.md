## Why

The legacy editor contains complete workflow lifecycle behavior, but most of that logic is still coupled to UI state and has not been fully implemented in the new backend projects. Delivering backend parity now is required so the API can become the source of truth for workflow creation, validation, execution, and state transitions.

## What Changes

- Implement backend workflow lifecycle logic currently represented in legacy components/services, including create/update/delete/list/validate/execute flows and execution state persistence.
- Move workflow and input normalization/validation concerns from UI-coupled legacy code into backend services and application handlers.
- Complete application-layer command/query handlers so they orchestrate core services and repositories instead of returning shell DTOs.
- Expand API contracts and endpoint behavior to support workflow lifecycle operations with predictable validation and execution error handling.
- Add integration and unit tests that lock in parity-critical behavior and prevent regressions while legacy UI coupling is removed.

## Capabilities

### New Capabilities
- `legacy-workflow-backend-parity`: Backend capability that provides full workflow lifecycle behavior and execution semantics equivalent to current legacy implementation, exposed through the new API/application/core/infrastructure stack.

### Modified Capabilities
- `api-surface`: Extend workflow endpoints/contracts to enforce consistent request validation, error payloads, and execution semantics.
- `workflow-lifecycle`: Add explicit backend requirements for workflow lifecycle state transitions, execution recording, and service-level orchestration.
- `core-engine`: Add requirements for workflow registration/execution services and schema validation integration at backend boundaries.
- `infrastructure`: Add requirements for durable persistence of workflow definitions and execution outcomes used by lifecycle operations.

## Impact

- Affected code: src/RulesEngine.API, src/RulesEngine.Application, src/RulesEngine.Core, src/RulesEngine.Infrastructure, src/RulesEngine.Editor.Shared, src/RulesEngine.Tests.
- Affected legacy reference: Legacy/src/RulesEngineEditor workflow-related models/services/pages used as parity baseline.
- APIs: Workflow CRUD, validation, and execution endpoints plus response/error contracts.
- Data: Workflow and execution state persistence behavior in RulesEngineEditorDbContext-backed stores.
- Testing: New/expanded API integration and core/application unit tests for parity coverage.
