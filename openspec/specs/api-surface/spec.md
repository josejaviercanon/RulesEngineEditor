# api-surface Specification

## Purpose
Define the externally visible Minimal API behavior for workflow lifecycle operations, HTTP semantics, and response contract expectations.
## Requirements
### Requirement: Workflow CRUD Endpoints
The system SHALL expose HTTP endpoints for workflow lifecycle operations and SHALL enforce consistent request validation, response payloads, and error contracts across create, read, update, delete, validate, and execute routes. Request and response payloads SHALL embed a typed `WorkflowDto` (with full `Rules`, `GlobalParams`, etc.) rather than a raw JSON string, and workflow responses SHALL surface version metadata for the active revision. The execute route SHALL accept a typed `Inputs` array and return a typed `Results` collection.

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

