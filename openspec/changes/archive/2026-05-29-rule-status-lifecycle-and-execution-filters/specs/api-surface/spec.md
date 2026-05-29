## MODIFIED Requirements

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
