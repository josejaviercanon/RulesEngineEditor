## Why

The current backend uses a flat `WorkflowDto` with a raw `RuleJson` string and minimal fields (`Name`, `Expression`, `RuleJson`, `Version`, `IsActive`), which does not reflect the full RulesEngine library model. This blocks typed CRUD, server-side schema validation, compilation error checking, and typed execution with user-supplied inputs — all prerequisites for building the visual LogicFlow.js workflow editor UI in a subsequent iteration.

## What Changes

- Replace the raw `RuleJson` string approach with a full typed DTO hierarchy that mirrors `RulesEngine.Models` (`Workflow`, `Rule`, `ScopedParam`, `RuleActions`, `ActionInfo`, etc.)
- Introduce `RuleDto`, `ScopedParamDto`, `RuleActionsDto`, `ActionInfoDto`, `RuleParameterDto`, and `RuleResultDto` in `RulesEngine.Application.Dtos`
- Replace `WorkflowRequest` / `WorkflowResponse` in `RulesEngine.API.Contracts` with records that embed the full model DTO (not raw JSON)
- Add JSON round-trip capability: serialize/deserialize the full model to/from the canonical RulesEngine JSON format (compatible with `workflow-list-schema.json`)
- Add a dedicated `/api/workflows/validate` endpoint that compiles the workflow against the RulesEngine library and reports C# expression errors without persisting
- Add typed test-execution support: `/api/workflows/{id}/execute` accepts named `RuleParameter` inputs (name + JSON-serialized value) and returns typed per-rule results
- Update AutoMapper/manual mappings to bridge `WorkflowDto` ↔ `Workflow` (RulesEngine model)

## Capabilities

### New Capabilities
- `workflow-full-dto-model`: Complete DTO hierarchy — `WorkflowDto` (with `Rules`, `GlobalParams`), `RuleDto` (with `LocalParams`, nested `Rules`, `Actions`), `ScopedParamDto`, `RuleActionsDto`, `ActionInfoDto` — replacing the flat raw-JSON model
- `workflow-model-json-schema`: JSON import/export round-trip (serialize DTO → RulesEngine JSON; deserialize RulesEngine JSON → DTO) and schema compatibility with `workflow-list-schema.json`
- `workflow-compile-validate`: POST `/api/workflows/validate` endpoint — accepts a full `WorkflowDto`, attempts to add it to a transient RulesEngine instance, returns compilation errors or success; no persistence
- `workflow-test-execution`: Typed test execution — `ExecuteWorkflowRequest` carries `IReadOnlyList<RuleParameterDto>` (name + JSON value), response carries per-rule `RuleResultDto` (rule name, success, error message, nested results, action output)

### Modified Capabilities
- `api-surface`: HTTP request/response contracts replace `RuleJson: string` with embedded `WorkflowDto`; `ExecuteWorkflowRequest` gains typed `Inputs` collection; `ExecuteWorkflowResponse` gains typed `Results` collection
- `workflow-lifecycle`: CRUD operations (create, read, update, list) use the full typed model; persistence serializes/deserializes the model via the JSON round-trip capability

## Impact

- `RulesEngine.Application.Dtos`: add `RuleDto`, `ScopedParamDto`, `RuleActionsDto`, `ActionInfoDto`, `RuleParameterDto`, `RuleResultDto`; update `WorkflowDto` and `ExecuteWorkflowResultDto`
- `RulesEngine.Application.Mapping`: add/update AutoMapper profile(s) for `Workflow` ↔ `WorkflowDto` and `Rule` ↔ `RuleDto`
- `RulesEngine.API.Contracts`: replace `WorkflowRequest`/`WorkflowResponse` with full-model records; extend `ExecuteWorkflowRequest`/`ExecuteWorkflowResponse`
- `RulesEngine.API`: add `/api/workflows/validate` route; update execute handler to unpack typed inputs
- `RulesEngine.Core` / `RulesEngine.Application.Handlers`: update command/query handlers to map from DTO → `Workflow` model when calling the RulesEngine library
- No changes to the RulesEngine library itself (it is consumed as-is from `RulesEngine/src/RulesEngine`)
- No UI changes in this iteration (LogicFlow.js editor is a subsequent change)
