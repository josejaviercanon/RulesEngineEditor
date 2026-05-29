## Why

The backend currently does not model rule lifecycle status as a first-class concern, which prevents users from explicitly controlling whether a rule is draft, disabled, or production-ready. We need explicit status semantics now to support safe authoring (including draft save with compile errors), deterministic execution behavior, and operationally consistent failure transitions.

## What Changes

- Add a persisted `Status` field to the rule entity and database model with allowed values: `draft`, `failed`, `disabled`, `production`.
- Define default behavior so all newly created rules start as `draft`.
- Allow saving rules with compilation/validation failures when status is `draft`.
- Enforce user-driven status authority:
  - only user action can set `disabled`
  - only user action can set `production`, and only when validations pass.
- Add automatic system transitions to `failed` when rule compilation or execution fails and status is not `draft`/`disabled`, including active production rules that fail at runtime.
- Define dry-run/test execution behavior over active workflow version rules:
  - include `production`, `draft`, `failed`
  - exclude `disabled`
  - support filtering by status set (for example only `failed`, only `draft`, only `production`, or all three).
- Extend API contracts/endpoints to accept status-aware execution filters and expose resulting status transitions.

## Capabilities

### New Capabilities
- `rule-status-lifecycle`: Rule status model, authority rules, and automatic transition behavior across save/validate/execute flows.

### Modified Capabilities
- `workflow-compile-validate`: Validation outcomes must interact with rule status rules (draft-save permissive behavior and failure transitions).
- `workflow-test-execution`: Dry-run execution must support status inclusion filters and status-specific transition/reporting behavior.
- `workflow-lifecycle`: Active workflow execution semantics must include runtime transition from production to failed on execution failure.
- `workflow-full-dto-model`: Rule DTOs/contracts must include persisted status and status transition metadata where applicable.
- `api-surface`: API endpoints must support status-aware updates and execution filter parameters.

## Impact

- Affected code: `src/RulesEngine.Core` rule domain models and validation/execution services, `src/RulesEngine.Application` command/query DTOs and handlers, `src/RulesEngine.Infrastructure` persistence/migrations, and `src/RulesEngine.API` endpoint contracts/controllers.
- API changes: request/response contracts for rule create/update, validation/compilation, and dry-run execution gain rule status semantics and status filters.
- Data changes: database migration required to add rule status column with default `draft` and constraints/index updates as needed.
- Testing: new and updated unit/integration tests for status transitions, validation gates, runtime failures, and execution filtering behavior.
