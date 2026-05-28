# workflow-compile-validate Specification

## Purpose
TBD - created by archiving change workflow-model-backend-dtos. Update Purpose after archive.
## Requirements
### Requirement: Compile-validate endpoint checks workflow expressions without persistence
The system SHALL expose a POST `/api/workflows/validate` endpoint that accepts a `WorkflowDto`, runs structural FluentValidation and then attempts to compile the workflow in a transient RulesEngine instance, returning a structured result with all errors or a success indicator — without persisting the workflow.

#### Scenario: Valid workflow with compilable expressions returns success
- **WHEN** POST /api/workflows/validate is called with a `WorkflowDto` whose expressions are valid C# lambda expressions
- **THEN** the response is HTTP 200 with `{ "IsValid": true, "Errors": [] }`

#### Scenario: Workflow with structural validation errors returns failure
- **WHEN** POST /api/workflows/validate is called with a `WorkflowDto` missing required fields (e.g., empty `WorkflowName` or `RuleName`)
- **THEN** the response is HTTP 422 with `{ "IsValid": false, "Errors": ["<validation error message>", ...] }`

#### Scenario: Workflow with invalid C# expression returns compile error
- **WHEN** POST /api/workflows/validate is called with a `WorkflowDto` whose `Rule.Expression` contains an invalid C# expression (type mismatch, undefined identifier, syntax error)
- **THEN** the response is HTTP 422 with `{ "IsValid": false, "Errors": ["<compile error message>", ...] }` describing the expression fault

#### Scenario: Validate endpoint does not persist the workflow
- **WHEN** POST /api/workflows/validate is called with any payload (valid or invalid)
- **THEN** no workflow record is written to or modified in the persistence store

### Requirement: ValidateWorkflowCommandHandler uses two-phase validation
The system SHALL implement `ValidateWorkflowCommandHandler` to first run FluentValidation (`WorkflowsValidator`) and, if structural validation passes, attempt to instantiate a transient `RulesEngine.RulesEngine` with the workflow to surface compile-time expression errors.

#### Scenario: Structural validation failure short-circuits compile check
- **WHEN** the workflow DTO fails FluentValidation
- **THEN** the handler returns the FluentValidation errors without attempting RulesEngine instantiation

#### Scenario: Compile error is captured and returned as structured error list
- **WHEN** the workflow DTO passes FluentValidation but RulesEngine throws during initialization
- **THEN** the handler catches the exception and returns it as a non-empty `Errors` collection with `IsValid: false`

