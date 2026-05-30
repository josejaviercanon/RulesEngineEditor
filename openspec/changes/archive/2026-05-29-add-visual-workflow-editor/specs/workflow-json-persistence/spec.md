## ADDED Requirements

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
