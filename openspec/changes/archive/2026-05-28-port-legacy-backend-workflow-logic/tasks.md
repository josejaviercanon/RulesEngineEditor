## 1. Legacy Parity Mapping and Contract Baseline

- [x] 1.1 Inventory backend-relevant legacy workflow behaviors (CRUD, validation, execution, input normalization, error handling) and map each behavior to target layers in `RulesEngine.API`, `RulesEngine.Application`, `RulesEngine.Core`, and `RulesEngine.Infrastructure`.
- [x] 1.2 Define parity acceptance checklist from spec scenarios and convert it into test case names for unit/integration coverage.
- [x] 1.3 Confirm API contract compatibility boundaries for workflow and execution responses, documenting additive vs potentially breaking changes.

## 2. Application and Core Orchestration

- [x] 2.1 Replace shell command/query handler behavior with repository-backed lifecycle orchestration for create, read, update, delete workflows.
- [x] 2.2 Implement application-layer validation/execution orchestration that enforces schema validation before rule execution.
- [x] 2.3 Extend core execution service usage to return deterministic execution result mapping for API response contracts.
- [x] 2.4 Add/adjust application and core unit tests for happy path and failure path scenarios defined in specs.

## 3. API Surface and Error Contract Alignment

- [x] 3.1 Refactor workflow minimal API endpoints to delegate orchestration to application/core services and keep endpoint logic thin.
- [x] 3.2 Standardize structured validation and execution error payloads across workflow lifecycle endpoints.
- [x] 3.3 Ensure execute endpoint behavior clearly distinguishes dry-run vs persisted execution, including execution ID semantics.
- [x] 3.4 Add or update API integration tests for CRUD, validation failures, dry-run execution, and persisted execution behavior.

## 4. Infrastructure Persistence Completion

- [x] 4.1 Complete repository operations and entity mapping needed for full workflow lifecycle parity.
- [x] 4.2 Ensure execution state persistence captures workflow ID, timestamps, success flag, and serialized result payload for non-dry-run paths.
- [x] 4.3 Add infrastructure-level integration coverage for workflow definition persistence and execution state persistence rules.
- [x] 4.4 Validate migration/backward compatibility assumptions for any schema/entity changes required by parity behavior.

## 5. End-to-End Verification and Rollout Readiness

- [x] 5.1 Run backend build and test suites (`RulesEngine.API`, `RulesEngine.Application`, `RulesEngine.Core`, `RulesEngine.Infrastructure`, `RulesEngine.Tests`) and resolve regressions.
- [x] 5.2 Execute parity checklist against legacy-referenced behaviors and confirm each mapped scenario is satisfied by backend implementation.
- [x] 5.3 Document rollout and rollback steps for persistence and API behavior changes in implementation notes.
- [x] 5.4 Prepare follow-up backlog items for explicitly out-of-scope concerns (retention policy, concurrency semantics, optional execute payload modes).
