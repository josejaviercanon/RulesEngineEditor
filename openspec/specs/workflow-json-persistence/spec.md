# workflow-json-persistence Specification

## Purpose
TBD - created by syncing change persist-workflow-and-rule-json. Update Purpose after implementation details are finalized.

## Requirements
### Requirement: Workflow entities MUST persist canonical workflow JSON
The system SHALL persist the full RulesEngine workflow document in `Workflow.WorkflowJson` such that one workflow row stores the complete JSON payload including all rules.

#### Scenario: Save full workflow document on create
- **WHEN** a workflow is created from a DTO or API payload containing a workflow with `N` rules
- **THEN** `WorkflowJson` stores a valid JSON document representing that same workflow and all `N` rules

#### Scenario: Preserve canonical workflow shape
- **WHEN** a workflow document is persisted to `WorkflowJson`
- **THEN** the JSON remains semantically equivalent to the RulesEngine workflow format (`WorkflowName` + `Rules[]`) used by runtime loading

### Requirement: Rule entities MUST persist per-rule JSON with plain-text expression
The system SHALL persist each rule revision in `Rule.RuleJson`, and each persisted rule JSON MUST include the rule `Expression` as plain text.

#### Scenario: Save rule JSON with expression
- **WHEN** a rule is created or updated with an expression string
- **THEN** `RuleJson` contains a JSON object for that rule where `Expression` equals the same plain-text expression value

#### Scenario: Rule JSON remains available for retrieval
- **WHEN** rule data is retrieved for workflow management or diagnostics
- **THEN** the system can return or reconstruct rule representations from `RuleJson` without losing `Expression` fidelity

### Requirement: Persistence layer MUST keep workflow and rule JSON consistent with structured fields
The system SHALL enforce write-path consistency so structured workflow/rule fields and canonical JSON fields represent the same logical revision.

#### Scenario: Update synchronizes both representations
- **WHEN** a workflow or rule is updated through backend commands
- **THEN** both structured columns and JSON columns are updated in the same transaction to represent the same versioned state

### Requirement: Visual editor save regenerates canonical JSON representations
The system SHALL regenerate and persist `Workflow.WorkflowJson` and each `Rule.RuleJson` from the canonical in-memory workflow model produced by visual editor save requests.

#### Scenario: Save from visual editor rewrites workflow and rule JSON
- **WHEN** a visual editor save request is processed
- **THEN** `WorkflowJson` is regenerated from the current workflow model
- **AND** each persisted rule revision has regenerated `RuleJson` matching current structured fields

### Requirement: Draft save with validation errors preserves JSON parseability
The system SHALL allow save-as-draft when business validation fails, but MUST reject persistence when generated `WorkflowJson` or `RuleJson` is not syntactically valid JSON.

#### Scenario: Draft save succeeds with valid JSON and domain errors
- **WHEN** validation reports domain/compile errors and user selects save-as-draft
- **THEN** persistence succeeds if generated workflow/rule JSON is syntactically valid

#### Scenario: Draft save fails when generated JSON is malformed
- **WHEN** save-as-draft is requested and generated JSON serialization fails correctness checks
- **THEN** the system rejects persistence and returns actionable errors
