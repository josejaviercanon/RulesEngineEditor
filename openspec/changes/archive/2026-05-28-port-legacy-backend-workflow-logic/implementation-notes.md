## Legacy Parity Mapping (Task 1.1)

- Legacy workflow CRUD state transitions (UI-triggered) -> backend command/query handlers in RulesEngine.Application (`CreateWorkflowCommandHandler`, `GetWorkflowByIdQueryHandler`, `ListWorkflowsQueryHandler`, `UpdateWorkflowCommandHandler`, `DeleteWorkflowCommandHandler`).
- Legacy workflow validation before execution -> backend validator orchestration in `ValidateWorkflowCommandHandler` and `ExecuteWorkflowCommandHandler`.
- Legacy rule execution path (`RulesEngine` invocation + result projection) -> backend execution orchestration in `ExecuteWorkflowCommandHandler` using `IRulesEngineWorkflowService`.
- Legacy execution persistence behavior (non-dry-run) -> infrastructure persistence via `IExecutionStateRepository` + `ExecutionStateRepository`.
- Legacy error handling normalization -> API-level structured `ValidationErrorResponse` and `ExecutionErrorResponse` mapping from application result DTOs.

## Parity Acceptance Checklist (Task 1.2, 5.2)

- [x] CRUD create/read/update/delete workflow lifecycle works end-to-end via API integration tests.
- [x] Validation endpoint returns structured errors for invalid workflow JSON.
- [x] Execute endpoint supports dry-run and persisted modes with distinct semantics.
- [x] Persisted execution writes execution state with workflow identity.
- [x] Invalid schema version blocks execution and returns validation error code.
- [x] Application handlers execute workflow lifecycle orchestration without endpoint-local repository logic.

Mapped tests:
- `WorkflowApiIntegrationTests.WorkflowCrudEndpoints_ShouldSupportCreateReadUpdateDelete`
- `WorkflowApiIntegrationTests.ValidateEndpoint_ShouldReturnStructuredErrorsForInvalidPayload`
- `WorkflowApiIntegrationTests.ExecuteEndpoint_ShouldSupportDryRunAndPersistedExecution`
- `WorkflowApiIntegrationTests.ExecuteEndpoint_ShouldReturnValidationErrorForInvalidSchemaVersion`
- `WorkflowApplicationHandlersTests.CreateWorkflowHandler_ShouldPersistWorkflowUsingRepository`
- `WorkflowApplicationHandlersTests.ValidateWorkflowHandler_ShouldReturnResolvedVersionAndErrors`
- `WorkflowApplicationHandlersTests.ExecuteWorkflowHandler_DryRun_ShouldNotPersistExecutionState`
- `WorkflowInfrastructurePersistenceTests.WorkflowRepository_ShouldPersistAndLoadWorkflowDefinition`
- `WorkflowInfrastructurePersistenceTests.ExecutionStateRepository_ShouldPersistExecutionStateRecord`

## API Compatibility Boundaries (Task 1.3)

Additive/non-breaking:
- Endpoint routes remain unchanged.
- Response contract shapes for workflow CRUD and execute success remain unchanged.
- Validation and execution error payloads continue using existing contract records.
- Internal orchestration moved to application layer without external endpoint signature changes.

Potentially breaking (none introduced in this implementation):
- No route removals.
- No required request field additions.
- No renames of response fields.

## Migration and Backward Compatibility Assumptions (Task 4.4)

- Persistence model remains compatible with existing workflow and execution state entities.
- Added `IExecutionStateRepository` abstraction does not change storage schema.
- Execution persistence remains write-on-real-run (`dryRun=false`) and no-op on dry-run.
- No EF migration required for this change set.

## Rollout and Rollback Notes (Task 5.3)

Rollout:
1. Deploy backend with application-handler orchestration enabled (default path).
2. Run post-deploy smoke tests for CRUD, validate, dry-run execute, persisted execute.
3. Confirm execution-state writes for persisted runs.

Rollback:
1. Revert backend deployment to prior commit.
2. Because schema did not change, rollback does not require DB migration rollback.
3. Re-run smoke tests against previous version.

## Follow-up Backlog (Task 5.4)

- Add execution history retention policy (TTL/capped history).
- Add optimistic concurrency/version conflict support for workflow updates.
- Evaluate optional ad-hoc inline execute payload mode in addition to ID-based execution.
- Add performance benchmark for execution result serialization under larger rule sets.
