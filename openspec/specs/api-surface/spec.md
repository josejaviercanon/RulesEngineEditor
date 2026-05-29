# api-surface Specification

## Purpose
Define the externally visible Minimal API behavior for workflow lifecycle operations, HTTP semantics, and response contract expectations.
## Requirements
### Requirement: Workflow CRUD Endpoints
The system SHALL expose HTTP endpoints for workflow lifecycle operations and SHALL enforce consistent request validation, response payloads, and error contracts across create, read, update, delete, validate, and execute routes. Request and response payloads SHALL embed a typed `WorkflowDto` (with full `Rules`, `GlobalParams`, etc.) rather than a raw JSON string, and workflow responses SHALL surface version metadata for the active revision. Rule payloads SHALL include rule `Status` with allowed values `draft|failed|disabled|production`, defaulting to `draft` for newly created rules. The execute route SHALL accept a typed `Inputs` array, a status filter parameter for `draft|failed|production` selection, and return a typed `Results` collection with status transition metadata.

#### Scenario: CRUD operations use consistent contracts with full model
- **WHEN** requests are made to /api/workflows for create, read, update, or delete operations
- **THEN** responses include a fully typed `WorkflowDto` payload (not a raw `RuleJson` field) alongside stable lifecycle metadata fields and HTTP status semantics aligned to operation outcome

#### Scenario: Versioned workflow responses surface active revision metadata
- **WHEN** a workflow response is returned for a workflow identity with multiple retained revisions
- **THEN** the response identifies the active revision and includes its current version number and active-state metadata

#### Scenario: Validation errors are structurally consistent
- **WHEN** a workflow request fails schema or contract validation on any workflow lifecycle endpoint
- **THEN** the API returns a structured error payload containing schema version context and an errors collection

#### Scenario: Execute route distinguishes dry-run and persisted execution
- **WHEN** POST /api/workflows/{id}/execute is called with dryRun=true or dryRun=false and an Inputs array
- **THEN** the API returns per-rule typed execution results with explicit dry-run and persistence indicators and includes execution identity only for persisted runs

#### Scenario: Validate route exposes compile-check without persistence
- **WHEN** POST /api/workflows/validate is called with a WorkflowDto payload
- **THEN** the API returns a structured validation result with IsValid and Errors and does not write to the persistence store

#### Scenario: Create workflow defaults rule status to draft
- **WHEN** POST /api/workflows receives rules without explicit `Status`
- **THEN** the API persists and returns those rules with `Status: "draft"`

#### Scenario: Execute route supports explicit status filtering
- **WHEN** POST /api/workflows/{id}/execute is called with a status filter containing one or more of `draft`, `failed`, `production`
- **THEN** only rules in the requested statuses are evaluated, and `disabled` rules are excluded

### Requirement: Workflow version management endpoints
The system SHALL expose HTTP endpoints to manage both workflow revisions and rule revisions within a workflow context. The API MUST allow listing rule versions for a `RuleGuidId`, activating a specific retained rule version, and querying workflow rules in active-only, latest-per-rule, or history-inclusive modes while preserving retained revision history.

#### Scenario: List rule revisions for a rule identity
- **WHEN** the client submits a request to list versions for a `RuleGuidId`
- **THEN** the API returns all retained versions in version order and identifies which version is active

#### Scenario: Activate specific rule revision
- **WHEN** the client submits a request to activate version 8 for a `RuleGuidId` that currently has version 10 active
- **THEN** the API marks version 8 active, marks other versions inactive for that `RuleGuidId`, and returns updated active-version metadata

#### Scenario: Query workflow rules in active-only mode
- **WHEN** the client requests workflow rules with active-only mode
- **THEN** the API returns one active rule revision per `RuleGuidId` and does not return duplicate logical rules

#### Scenario: Query workflow rules in latest-per-rule mode
- **WHEN** the client requests workflow rules with latest-per-rule mode
- **THEN** the API returns one highest-version rule revision per `RuleGuidId` regardless of active flag

### Requirement: API supports UI workflow management actions for Home and Rules pages
The API SHALL provide contracts that allow the UI to load one-row-per-workflow listings, list workflow versions, activate selected workflow versions, and apply workflow enable/disable transitions with consistent validation responses.

#### Scenario: Home workflow list is loadable from API
- **WHEN** the UI requests workflow list data for Home or Rules pages
- **THEN** the API returns workflow entries that include Guid identity, active version metadata, workflow name, and enablement state required by the grid

#### Scenario: Workflow version list and activation are available for edit modal
- **WHEN** the UI requests versions for a workflow identity and activates a selected version
- **THEN** the API returns version history and applies activation so only one version remains active for that workflow identity

#### Scenario: Enable or disable returns actionable validation errors
- **WHEN** the UI requests enable/disable for a workflow version that violates lifecycle constraints
- **THEN** the API returns a structured validation response suitable for modal feedback

### Requirement: API returns rule expression validation feedback on save attempts
The API SHALL validate rule expression content on rule create/update operations and SHALL return user-consumable validation details when validation fails.

#### Scenario: Rule expression validation failure on create
- **WHEN** the UI submits a new rule with an invalid expression
- **THEN** the API rejects the request with structured validation messages describing expression errors

#### Scenario: Rule expression validation failure on update
- **WHEN** the UI submits an edit for an existing rule with an invalid expression
- **THEN** the API rejects the request with structured validation messages without mutating persisted rule data

