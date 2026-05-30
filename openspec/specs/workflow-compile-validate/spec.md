# workflow-compile-validate Specification

## Purpose
TBD - created by archiving change workflow-model-backend-dtos. Update Purpose after archive.
## Requirements
### Requirement: Compile-validate endpoint checks workflow expressions without persistence
The system SHALL expose a POST `/api/workflows/validate` endpoint that accepts a `WorkflowDto`, runs structural FluentValidation and then attempts to compile the workflow in a transient RulesEngine instance, returning a structured result with all errors or a success indicator — without persisting the workflow. The validation result SHALL include rule-level status evaluation metadata indicating whether each rule remains in its current status or must transition to `failed` under status policy when the workflow is later persisted.

#### Scenario: Valid workflow with compilable expressions returns success
- **WHEN** POST /api/workflows/validate is called with a `WorkflowDto` whose expressions are valid C# lambda expressions
- **THEN** the response is HTTP 200 with `{ "IsValid": true, "Errors": [] }`

#### Scenario: Workflow with structural validation errors returns failure
- **WHEN** POST /api/workflows/validate is called with a `WorkflowDto` missing required fields (e.g., empty `WorkflowName` or `RuleName`)
- **THEN** the response is HTTP 422 with `{ "IsValid": false, "Errors": ["<validation error message>", ...] }`

#### Scenario: Workflow with invalid C# expression returns compile error
- **WHEN** POST /api/workflows/validate is called with a `WorkflowDto` whose `Rule.Expression` contains an invalid C# expression (type mismatch, undefined identifier, syntax error)
- **THEN** the response is HTTP 422 with `{ "IsValid": false, "Errors": ["<compile error message>", ...] }` describing the expression fault

#### Scenario: Compile failure marks non-draft non-disabled rules for failed transition
- **WHEN** POST /api/workflows/validate is called for rules currently in `production` and compilation fails
- **THEN** the validation result includes rule-level transition metadata indicating those rules must transition to `failed` on persistence

#### Scenario: Compile failure does not force failed transition for draft or disabled
- **WHEN** POST /api/workflows/validate is called for rules in `draft` or `disabled` and compilation fails
- **THEN** the validation result indicates those rules remain in their current statuses while reporting compile errors

#### Scenario: Validate endpoint does not persist the workflow
- **WHEN** POST /api/workflows/validate is called with any payload (valid or invalid)
- **THEN** no workflow record is written to or modified in the persistence store

### Requirement: ValidateWorkflowCommandHandler uses two-phase validation
The system SHALL implement `ValidateWorkflowCommandHandler` to first run FluentValidation (`WorkflowsValidator`) and, if structural validation passes, attempt to instantiate a transient `RulesEngine.RulesEngine` with the workflow to surface compile-time expression errors, then evaluate status transition policy metadata for each rule based on compile outcomes.

#### Scenario: Structural validation failure short-circuits compile check
- **WHEN** the workflow DTO fails FluentValidation
- **THEN** the handler returns the FluentValidation errors without attempting RulesEngine instantiation

#### Scenario: Compile error is captured and returned as structured error list
- **WHEN** the workflow DTO passes FluentValidation but RulesEngine throws during initialization
- **THEN** the handler catches the exception and returns it as a non-empty `Errors` collection with `IsValid: false`

#### Scenario: Handler computes status-transition metadata from compile outcomes
- **WHEN** compile evaluation finishes for a workflow containing rules with mixed statuses
- **THEN** the handler returns per-rule status transition metadata consistent with policy (`production` -> `failed`; `draft` stays `draft`; `disabled` stays `disabled`; `failed` stays `failed`)

### Requirement: Rule expression validation endpoint returns user-consumable results
The system SHALL expose a rule-expression validation operation that evaluates a selected rule expression using RulesEngine-compatible validation and returns structured validity results without persistence side effects.

#### Scenario: Validate expression returns success for valid rule
- **WHEN** the client validates a rule expression that compiles and passes semantic checks
- **THEN** the response indicates `IsValid=true`
- **AND** no workflow or rule data is persisted or modified

#### Scenario: Validate expression returns errors for invalid rule
- **WHEN** the client validates a rule expression that fails syntax or semantic checks
- **THEN** the response indicates `IsValid=false`
- **AND** the response includes one or more validation error messages

### Requirement: Workflow validation is invocable from workflow action context
The system SHALL support explicit workflow-level validation for selected workflow rows and SHALL return structured success/error results for UI display.

#### Scenario: Validate selected workflow returns success
- **WHEN** the client validates a selected workflow with valid rules and expressions
- **THEN** the response indicates `IsValid=true`

#### Scenario: Validate selected workflow returns errors
- **WHEN** the client validates a selected workflow that contains invalid rule expressions or structural issues
- **THEN** the response indicates `IsValid=false`
- **AND** the response includes workflow/rule-level validation errors for user display

### Requirement: Save workflow operation supports validation warning decision flow
The system SHALL perform dry-run workflow validation before persisting visual editor saves and MUST return a warning decision contract when validation errors are present.

#### Scenario: Save pre-check returns warning decision options
- **WHEN** a save request fails dry-run validation
- **THEN** the response includes validation errors and explicit decision options for `save-as-draft` or `cancel`

#### Scenario: Cancel after warning does not persist
- **WHEN** the client receives a warning decision response and chooses cancel
- **THEN** no workflow or rule persistence occurs

### Requirement: Draft override save preserves validation diagnostics
When a client selects `save-as-draft` after validation warnings, the system SHALL persist the workflow and SHALL return validation diagnostics to support user remediation.

#### Scenario: Save as draft returns persisted result with warnings
- **WHEN** save-as-draft is requested after validation warnings
- **THEN** the workflow is persisted as draft-compatible state
- **AND** the response includes warning/validation details for UI display

