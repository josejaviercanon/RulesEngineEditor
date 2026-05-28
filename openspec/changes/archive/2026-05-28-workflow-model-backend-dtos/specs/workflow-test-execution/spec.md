## ADDED Requirements

### Requirement: Execute endpoint accepts structured named test inputs
The system SHALL extend the POST `/api/workflows/{id}/execute` endpoint to accept an `Inputs` array of `RuleParameterDto` objects (name + JSON value) in the request body, which are passed to the RulesEngine as `RuleParameter[]`.

#### Scenario: Execute with named inputs evaluates rules against those inputs
- **WHEN** POST /api/workflows/{id}/execute is called with `Inputs: [{ "Name": "order", "ValueJson": "{\"total\": 500}" }]`
- **THEN** the RulesEngine evaluates all rules in the workflow using `order` as the named input and returns results

#### Scenario: Execute with empty Inputs array uses no inputs
- **WHEN** POST /api/workflows/{id}/execute is called with `Inputs: []`
- **THEN** the RulesEngine evaluates with no inputs (valid for rules that reference no inputs) and returns results

#### Scenario: Execute with invalid ValueJson returns parse error
- **WHEN** POST /api/workflows/{id}/execute is called with an `Inputs` entry whose `ValueJson` is not valid JSON
- **THEN** the response is HTTP 422 with a structured error indicating which input parameter has invalid JSON

### Requirement: Execute response carries typed per-rule results
The system SHALL return a typed `Results` collection of `RuleResultDto` in the execute response, replacing or supplementing the opaque `ResultJson` string.

#### Scenario: Execute response includes per-rule outcome
- **WHEN** POST /api/workflows/{id}/execute completes successfully
- **THEN** the response includes `Results: [ { "RuleName": "...", "IsSuccess": true|false, "ExceptionMessage": null|"...", "SuccessEvent": null|"...", "ActionOutput": null|"...", "ChildResults": [...] }, ... ]`

#### Scenario: Execute response includes dry-run and persisted metadata
- **WHEN** POST /api/workflows/{id}/execute completes
- **THEN** the response includes `DryRun` (bool) and `Persisted` (bool) metadata fields alongside `Results`

### Requirement: ExecuteWorkflowCommandHandler maps RuleParameterDto to RuleParameter
The system SHALL implement `ExecuteWorkflowCommandHandler` to deserialize each `RuleParameterDto.ValueJson` into a `JsonElement`, wrap it in `new RuleParameter(Name, jsonElement)`, and pass the resulting array to `RulesEngine.ExecuteAllRulesAsync`.

#### Scenario: Handler maps inputs and calls ExecuteAllRulesAsync
- **WHEN** the handler receives a command with a non-empty `Inputs` collection
- **THEN** it constructs one `RuleParameter` per `RuleParameterDto` and passes all of them to `ExecuteAllRulesAsync`

#### Scenario: Handler maps RuleResultTree to RuleResultDto recursively
- **WHEN** `ExecuteAllRulesAsync` returns a list of `RuleResultTree`
- **THEN** the handler maps each tree node (including nested `ChildResults`) to `RuleResultDto` and includes them in the command result
