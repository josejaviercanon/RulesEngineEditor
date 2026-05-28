## 1. DTO Hierarchy (RulesEngine.Application.Dtos)

- [x] 1.1 Add `ScopedParamDto.cs` with `Name` and `Expression` string properties
- [x] 1.2 Add `ActionInfoDto.cs` with `Name` (string) and `Context` (`Dictionary<string, JsonElement>`) properties
- [x] 1.3 Add `RuleActionsDto.cs` with nullable `OnSuccess` and `OnFailure` (`ActionInfoDto`) properties
- [x] 1.4 Add `RuleDto.cs` with all rule fields: `RuleName`, `Operator`, `ErrorMessage`, `Enabled`, `RuleExpressionType`, `Expression`, `SuccessEvent`, `LocalParams` (`IReadOnlyList<ScopedParamDto>`), `Rules` (`IReadOnlyList<RuleDto>`), `Actions` (`RuleActionsDto`), `WorkflowsToInject` (`IReadOnlyList<string>`), `Properties` (`Dictionary<string, string>`)
- [x] 1.5 Update `WorkflowDto.cs`: replace `Expression` and `RuleJson` string fields with `WorkflowName` (string), `RuleExpressionType` (enum), `GlobalParams` (`IReadOnlyList<ScopedParamDto>`), `Rules` (`IReadOnlyList<RuleDto>`), `WorkflowsToInject` (`IReadOnlyList<string>`); retain `Id`, `Version`, `IsActive`, `EffectiveFromUtc`, `EffectiveToUtc`
- [x] 1.6 Add `RuleParameterDto.cs` with `Name` (string, required) and `ValueJson` (string, required) properties
- [x] 1.7 Add `RuleResultDto.cs` with `RuleName` (string), `IsSuccess` (bool), `ExceptionMessage` (string?), `SuccessEvent` (string?), `ActionOutput` (string?), and `ChildResults` (`IReadOnlyList<RuleResultDto>`) properties
- [x] 1.8 Update `ExecuteWorkflowResultDto.cs`: replace `ResultJson` string with `Results` (`IReadOnlyList<RuleResultDto>`); retain `Found`, `IsSuccess`, `DryRun`, `Persisted`, `ExecutionId`, `ErrorCode`, `ErrorMessage`, `Errors`

## 2. AutoMapper Profiles (RulesEngine.Application.Mapping)

- [x] 2.1 Create or update mapping profile: `ScopedParam` ↔ `ScopedParamDto` (bidirectional)
- [x] 2.2 Create or update mapping profile: `ActionInfo` ↔ `ActionInfoDto` — map `Context` from `Dictionary<string, object>` to `Dictionary<string, JsonElement>` using `JsonSerializer.SerializeToElement` for each value
- [x] 2.3 Create or update mapping profile: `RuleActions` ↔ `RuleActionsDto` (bidirectional)
- [x] 2.4 Create or update mapping profile: `Rule` ↔ `RuleDto` (bidirectional, recursive for nested `Rules`)
- [x] 2.5 Create or update mapping profile: `Workflow` ↔ `WorkflowDto` (bidirectional) — map `WorkflowName` ↔ `WorkflowName`, `GlobalParams`, `Rules`, `WorkflowsToInject`, `RuleExpressionType`
- [x] 2.6 Create mapping: `RuleResultTree` → `RuleResultDto` — map `Rule.RuleName`, `IsSuccess`, `ExceptionMessage`, `Rule.SuccessEvent`, `ActionResult.Output` (serialized to string) → `ActionOutput`, and recursive `ChildResults`
- [x] 2.7 Verify AutoMapper configuration compiles and passes `AssertConfigurationIsValid()` in a test or startup check

## 3. API Contracts (RulesEngine.API.Contracts)

- [x] 3.1 Replace `WorkflowRequest` record: remove `Expression`, `RuleJson`, `Version`, `IsActive`, `EffectiveFromUtc`, `EffectiveToUtc`, `SchemaVersion` flat fields; embed `WorkflowDto Workflow` and retain `int? SchemaVersion`
- [x] 3.2 Replace `WorkflowResponse` record: embed `WorkflowDto Workflow` alongside `Guid Id`, `int Version`, `bool IsActive`, `DateTimeOffset? EffectiveFromUtc`, `DateTimeOffset? EffectiveToUtc`
- [x] 3.3 Update `ExecuteWorkflowRequest` record: add `IReadOnlyList<RuleParameterDto> Inputs` (default empty list); retain `DryRun` and `SchemaVersion`
- [x] 3.4 Update `ExecuteWorkflowResponse` record: add `IReadOnlyList<RuleResultDto> Results`; retain `DryRun`, `SchemaVersion`, `Persisted`, `WasSuccessful`, `ExecutionId`
- [x] 3.5 Add `ValidateWorkflowRequest` record with `WorkflowDto Workflow` (no `SchemaVersion` — structural validation is schema-agnostic with typed DTOs)
- [x] 3.6 Add `ValidateWorkflowResponse` record with `bool IsValid` and `IReadOnlyList<string> Errors`; note: HTTP status is always 200 — `IsValid=false` is a domain result, not an HTTP error

## 4. JSON Serialization Configuration

- [x] 4.1 Configure `System.Text.Json` options in `RulesEngine.API` `Program.cs` to use `JsonStringEnumConverter` for `RuleExpressionType` and `RuleErrorType` enums
- [x] 4.2 Add `JsonPropertyName` attributes to `WorkflowDto`, `RuleDto`, `ScopedParamDto`, `RuleActionsDto`, `ActionInfoDto` to match the RulesEngine canonical property names (`WorkflowName`, `RuleName`, `GlobalParams`, etc.) for round-trip JSON compatibility
- [x] 4.3 Write a unit test: serialize a `WorkflowDto` → JSON string → deserialize to `WorkflowDto` and assert all fields are preserved (round-trip test)
- [x] 4.4 Write a unit test: deserialize the sample workflow payload fixture (`Legacy/demo/RulesEngineEditorWebAssembly/wwwroot/sample-data/discount.json`) and assert the resulting `WorkflowDto` list has expected `WorkflowName`, rule count, and expression values

## 5. Validate Endpoint (structural only — expression errors surface via execute results)

<!--
  DESIGN NOTES (from explore analysis):
  - WorkflowsValidator is INTERNAL to the RulesEngine assembly — cannot be instantiated directly.
  - The only public path to structural validation is: create a transient RulesEngine instance,
    call AddOrUpdateWorkflow(workflow), and catch RuleValidationException (which IS public).
  - This is safe to do transiently: AddOrUpdateWorkflow only runs FluentValidation (no JIT compilation).
    The anti-pattern is transient engine + ExecuteAllRulesAsync. Structural-only validate is fast.
  - Expression compile errors (bad DLINQ syntax, unknown identifiers) cannot be detected without
    real input parameters because DLINQ compiles against the input types. They surface as
    RuleResultTree.ExceptionMessage in execute results (EnableExceptionAsErrorMessage=true by default).
  - The execute endpoint serves as the "compile validate" — run with test inputs, check ExceptionMessage.
    We do NOT need a separate compile-check endpoint.
-->
- [x] 5.1 Update `ValidateWorkflowCommandHandler`: map `WorkflowDto` → `RulesEngine.Models.Workflow` via AutoMapper; create a transient `new global::RulesEngine.RulesEngine()` instance, call `engine.AddOrUpdateWorkflow(workflow)`, catch `RuleValidationException ex` and return `{ IsValid: false, Errors: ex.Errors.Select(e => e.ErrorMessage) }`; on success return `{ IsValid: true, Errors: [] }`
- [x] 5.2 Remove `IWorkflowSchemaValidator` dependency from `ValidateWorkflowCommandHandler` — structural validation is now handled by `RuleValidationException` from `AddOrUpdateWorkflow`; also remove it from `ExecuteWorkflowCommandHandler` (replaced by `RuleValidationException` pattern)
- [x] 5.3 Add or update the POST `/api/workflows/validate` route in `Program.cs` to accept `ValidateWorkflowRequest` (contains `WorkflowDto Workflow`) and return `ValidateWorkflowResponse` with `bool IsValid` and `IReadOnlyList<string> Errors` (HTTP 200 always, `IsValid` signals outcome)
- [x] 5.4 Write unit tests for `ValidateWorkflowCommandHandler`: test structural failure (empty `WorkflowName`), test missing expression, test valid workflow; note: do NOT test expression compile errors here — those are tested via the execute endpoint

## 6. Execute Endpoint (workflow-test-execution — also surfaces expression compile errors)

<!--
  DESIGN NOTES (from explore analysis + architectural decision):
  - IRulesEngineWorkflowService now exposes ExecuteWorkflowAsync(Guid workflowId, Workflow workflowDefinition,
    RuleParameter[] ruleParameters). The handler loads the Workflow from the repository and passes it
    along with the DB id — the singleton service caches a compiled RulesEngine per workflowId.
  - Expression compile errors surface as RuleResultTree.ExceptionMessage (non-empty string, IsSuccess=false)
    because EnableExceptionAsErrorMessage=true and EnableExceptionAsErrorMessageForRuleExpressionParsing=true.
    Callers detect them by checking if any result has a non-null/non-empty ExceptionMessage.
  - ExecuteAllRulesAsync IS thread-safe. The singleton service handles concurrent requests correctly via
    ConcurrentDictionary<Guid, Lazy<RulesEngine>> — JIT compilation happens exactly once per workflow version.
-->
- [x] 6.1 Update `ExecuteWorkflowCommandHandler`: deserialize each `RuleParameterDto.ValueJson` using `JsonSerializer.Deserialize<JsonElement>`, construct `new RuleParameter(dto.Name, jsonElement)`, build `RuleParameter[]`; if any `ValueJson` fails deserialization return a structured 422 error identifying the invalid parameter
- [x] 6.2 In `ExecuteWorkflowCommandHandler`: deserialize the stored `WorkflowRecord.RuleJson` back to `WorkflowDto` (via `JsonSerializer.Deserialize<WorkflowDto>`), then map `WorkflowDto` → `RulesEngine.Models.Workflow` via AutoMapper
- [x] 6.3 In `ExecuteWorkflowCommandHandler`: call `await rulesEngineWorkflowService.ExecuteWorkflowAsync(workflowRecord.Id, workflow, ruleParameters, cancellationToken)` — the singleton service handles per-workflow JIT compilation caching; catch `RuleValidationException` here as well (structural errors found on first load)
- [x] 6.4 Map the returned `IReadOnlyList<RuleResultTree>` to `IReadOnlyList<RuleResultDto>` using AutoMapper (recursive mapping from step 2.6); note: results with non-empty `ExceptionMessage` indicate expression errors — include them in the response as-is so the caller can surface them
- [x] 6.5 Return `RuleResultDto` list in `ExecuteWorkflowResultDto.Results`; keep `ExecuteWorkflowResultDto.IsSuccess=true` for successful engine execution and use `WasSuccessful = results.All(r => r.IsSuccess)` for business-rule outcome; populate `ErrorCode/ErrorMessage` only when exceptions (e.g., `RuleValidationException`) are caught
- [x] 6.6 Write unit tests for `ExecuteWorkflowCommandHandler`: test with valid named inputs and valid expression, test with empty inputs (expect results with ExceptionMessage for input-referencing rules), test with invalid `ValueJson`, test `RuleValidationException` path

## 7. CRUD Handler Updates (workflow-lifecycle + cache invalidation)

<!--
  DESIGN NOTES:
  - IRulesEngineWorkflowService singleton maintains a ConcurrentDictionary<Guid, Lazy<RulesEngine>> cache.
  - Cache invalidation contract: handlers MUST call RefreshWorkflow after create/update and EvictWorkflow
    after delete. Failure to do so will cause the execute endpoint to use stale compiled rules.
  - WorkflowRecord.RuleJson stores the serialized WorkflowDto (full model, PascalCase JSON).
  - WorkflowRecord.Name is kept in sync with WorkflowDto.WorkflowName for query purposes.
  - WorkflowRecord.Expression field is left empty / unused (legacy field, not removed from schema).
-->
- [x] 7.1 Update `CreateWorkflowCommandHandler`: accept `WorkflowDto` in the command; map to `RulesEngine.Models.Workflow` via AutoMapper; try `AddOrUpdateWorkflow` on transient engine to catch structural errors early; serialize `WorkflowDto` to JSON via `JsonSerializer.Serialize` and store in `WorkflowRecord.RuleJson`; set `WorkflowRecord.Name = dto.WorkflowName`; after successful persist, call `rulesEngineWorkflowService.RefreshWorkflow(record.Id, workflow)` to pre-seed the cache
- [x] 7.2 Update `UpdateWorkflowCommandHandler`: accept `WorkflowDto` in the command; map to `Workflow`; validate structurally (same transient-engine pattern as 7.1); serialize to JSON and store; after successful persist, call `rulesEngineWorkflowService.RefreshWorkflow(record.Id, workflow)` to invalidate and replace the cached compiled engine
- [x] 7.3 Update `GetWorkflowByIdQueryHandler`: deserialize `WorkflowRecord.RuleJson` back to `WorkflowDto` using `JsonSerializer.Deserialize<WorkflowDto>` and return full model in response; no cache operation required
- [x] 7.4 Update `ListWorkflowsQueryHandler`: for each `WorkflowRecord`, deserialize `RuleJson` to `WorkflowDto`; return full model in list response; no cache operation required
- [x] 7.5 Update `DeleteWorkflowCommandHandler`: after successful delete from persistence, call `rulesEngineWorkflowService.EvictWorkflow(id)` to remove the cached engine; verify handler still compiles after contract changes

## 8. Integration and Regression Tests (RulesEngine.Tests)

- [x] 8.1 Add integration test: POST /api/workflows/validate with a valid `WorkflowDto` → assert HTTP 200 and `IsValid: true`
- [x] 8.2 Add integration test: POST /api/workflows/{id}/execute with a workflow containing an invalid expression and test inputs → assert HTTP 200, `WasSuccessful: false`, and non-empty `Results[0].ExceptionMessage`
- [x] 8.3 Add integration test: POST /api/workflows with full `WorkflowDto` (create) → GET /api/workflows/{id} → assert round-trip preserves all rules and params
- [x] 8.4 Add integration test: POST /api/workflows/{id}/execute with `Inputs` array → assert `Results` contains one `RuleResultDto` per rule with correct `RuleName` and `IsSuccess`
- [x] 8.5 Verify existing tests in `RulesEngineWorkflowServiceTests.cs` still pass after handler and contract changes
