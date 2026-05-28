## Context

The backend was scaffolded with a minimal `WorkflowDto` that stores a raw `RuleJson: string` blob and exposes flat fields (`Name`, `Expression`, `Version`, `IsActive`). This does not model the RulesEngine library's actual `Workflow`/`Rule` structure, making it impossible to perform typed validation, to compile-check expressions server-side, or to pass structured test inputs to the engine. The RulesEngine library (`RulesEngine/src/RulesEngine/Models/`) already defines the canonical model (`Workflow`, `Rule`, `ScopedParam`, `RuleActions`, `ActionInfo`, `RuleParameter`, `RuleResultTree`). All handler stubs exist in `RulesEngine.Application.Handlers`; `RulesEngine.API.Contracts` has the record types that need updating.

## Goals / Non-Goals

**Goals:**
- Define a complete DTO hierarchy in `RulesEngine.Application.Dtos` that mirrors the RulesEngine library model 1:1
- Replace the raw `RuleJson` approach in API contracts with embedded typed DTOs
- Provide JSON import/export round-trip compatibility with the canonical `workflow-list-schema.json` format
- Add a compile-validate endpoint that uses the RulesEngine library's own validator to check expressions without persistence
- Extend the execute endpoint to accept structured test inputs (`RuleParameterDto[]`) and return per-rule typed results
- Update AutoMapper/mapping profiles to bridge `WorkflowDto` ↔ `RulesEngine.Models.Workflow`

**Non-Goals:**
- Visual workflow editor UI (LogicFlow.js — subsequent iteration)
- EF Core persistence schema changes beyond what is needed to store the serialized workflow JSON
- Custom action support (no `CustomTypes` or `CustomActions` registration in this iteration)
- Authentication/authorization on the new endpoints
- Versioning strategy beyond the existing `Version` field

## Decisions

### Decision: DTO hierarchy mirrors the library model, not a flattened schema

**Choice**: `WorkflowDto` embeds `IReadOnlyList<RuleDto>` and `IReadOnlyList<ScopedParamDto>` directly, matching the `Workflow` model. `RuleDto` embeds `IReadOnlyList<RuleDto>` for nested rules.

**Rationale**: Maintains a 1:1 mapping that allows round-tripping to/from the library model without loss of information. A flattened schema would require a custom de-nesting step and would break JSON schema compatibility.

**Alternative considered**: Store the workflow as a JSON string and deserialize on demand — rejected because it bypasses typed validation, makes the API opaque, and defers compile errors to runtime.

### Decision: JSON import/export uses `System.Text.Json` with the same property names as the RulesEngine library

**Choice**: Serialize `WorkflowDto` using `System.Text.Json` with `JsonPropertyName` attributes matching the RulesEngine JSON schema (`WorkflowName`, `Rules`, `GlobalParams`, etc.). The mapping layer converts between DTO property naming (`Name` → `WorkflowName`) and API property naming.

**Rationale**: Ensures that JSON blobs exported by this API can be imported directly into other RulesEngine consumers (CLI, tests, other services) without transformation.

**Alternative considered**: Use a separate serialization model with camelCase — rejected because it creates drift from the official schema.

### Decision: Compile-validate uses a transient `RulesEngine` instance, not FluentValidation alone

**Choice**: The validate endpoint instantiates `new RulesEngine.RulesEngine(new[] { workflow })` in a try/catch and reports any `RuleException` or expression-compile errors. FluentValidation (`WorkflowsValidator`) runs first as a fast pre-check.

**Rationale**: FluentValidation catches structural errors (missing names, null expressions) but cannot catch C# expression compile errors (type mismatches, undefined identifiers). The transient engine catches those. The user explicitly asked for "no compilation errors in Workflow" as the test criterion.

**Alternative considered**: Surface only FluentValidation errors — rejected because it misses the most important class of errors (broken C# expressions).

### Decision: Test execution inputs are serialized as JSON strings in the DTO, not as typed `object`

**Choice**: `RuleParameterDto` carries `Name: string` and `ValueJson: string`. The handler deserializes `ValueJson` as a `JsonElement` (dynamic object) and passes it to `new RuleParameter(Name, value)`.

**Rationale**: The API boundary is HTTP/JSON; callers cannot express arbitrary C# types. Using `JsonElement` (deserialized from `ValueJson`) produces an anonymous-type-compatible object that RulesEngine can evaluate expressions against. This matches how the Legacy demo handled inputs.

**Alternative considered**: Accept `object` in the contract and rely on `System.Text.Json` deserialization — rejected because untyped `object` deserialization in STJ produces `JsonElement` anyway, but is less explicit about intent.

### Decision: `RuleResultDto` flattens only the fields needed for UI display

**Choice**: `RuleResultDto` carries `RuleName`, `IsSuccess`, `ExceptionMessage`, `SuccessEvent`, `ActionOutput` (serialized), and `ChildResults: IReadOnlyList<RuleResultDto>`. It does not include `Inputs` or `Rule` reference.

**Rationale**: The UI needs rule name, outcome, error, and nested results. Exposing the full `Rule` object in results creates duplication; `Inputs` is large and not needed for result display.

## Risks / Trade-offs

- [Risk: Nested rule depth] Deeply nested `Rule.Rules` trees serialize to large JSON payloads → Mitigation: no depth limit imposed now; document that UI should paginate or lazy-load deep trees.
- [Risk: `ActionInfo.Context` is `Dictionary<string, object>`] Arbitrary context values may not round-trip correctly through STJ → Mitigation: serialize `Context` as `Dictionary<string, JsonElement>` in the DTO and convert at the mapping layer; document limitation.
- [Risk: RulesEngine transient instance in validate] Creating and discarding `RulesEngine` per request has startup cost → Mitigation: acceptable for validate (not a hot path); cache is not shared between requests so no state pollution.
- [Risk: Existing handler stubs accept old contracts] Updating `WorkflowRequest`/`WorkflowResponse` is a breaking change for any in-flight client → Mitigation: no external clients exist yet; the API is internal/development-only at this stage.

## Migration Plan

1. Update `WorkflowDto` and add sibling DTOs in `RulesEngine.Application.Dtos` (no persistence schema change needed — existing `RuleJson` column will store the serialized full model JSON going forward).
2. Update AutoMapper profiles in `RulesEngine.Application.Mapping`.
3. Update `RulesEngine.API.Contracts` records to embed `WorkflowDto` instead of raw JSON fields.
4. Update handler implementations to use the new model.
5. Add the compile-validate endpoint and wire it to `ValidateWorkflowCommandHandler`.
6. Extend the execute endpoint / `ExecuteWorkflowCommandHandler` to accept and unpack typed inputs.
7. Run `RulesEngine.Tests` to confirm no regressions.

Rollback: all changes are within the `src/` projects which are not yet deployed externally. Reverting the PR restores the previous state.

## Open Questions

- Should the `WorkflowDto.Id` (persistence `Guid`) be included in the API response for the validate endpoint (no persistence), or should validate return only validation status? → Decision deferred to tasks; propose returning only `{ IsValid, Errors }`.
- Should `RuleDto.Properties` (custom tags) be exposed in the DTO or silently dropped? → Propose exposing as `Dictionary<string, string>` (values coerced to string) to avoid untyped `object` at the API boundary; document in spec.
