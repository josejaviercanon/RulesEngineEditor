## MODIFIED Requirements

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
