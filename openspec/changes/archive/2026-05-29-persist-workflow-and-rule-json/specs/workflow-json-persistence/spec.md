## ADDED Requirements

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
