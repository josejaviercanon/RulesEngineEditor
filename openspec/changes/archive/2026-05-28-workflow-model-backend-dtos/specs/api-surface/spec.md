## MODIFIED Requirements

### Requirement: Workflow CRUD Endpoints
The system SHALL expose HTTP endpoints for workflow lifecycle operations and SHALL enforce consistent request validation, response payloads, and error contracts across create, read, update, delete, validate, and execute routes. Request and response payloads SHALL embed a typed `WorkflowDto` (with full `Rules`, `GlobalParams`, etc.) rather than a raw JSON string. The execute route SHALL accept a typed `Inputs` array and return a typed `Results` collection.

#### Scenario: CRUD operations use consistent contracts with full model
- **WHEN** requests are made to /api/workflows for create, read, update, or delete operations
- **THEN** responses include a fully typed `WorkflowDto` payload (not a raw `RuleJson` field) alongside stable lifecycle metadata fields and HTTP status semantics aligned to operation outcome

#### Scenario: Validation errors are structurally consistent
- **WHEN** a workflow request fails schema or contract validation on any workflow lifecycle endpoint
- **THEN** the API returns a structured error payload containing schema version context and an errors collection

#### Scenario: Execute route distinguishes dry-run and persisted execution
- **WHEN** POST /api/workflows/{id}/execute is called with dryRun=true or dryRun=false and an Inputs array
- **THEN** the API returns per-rule typed execution results with explicit dry-run and persistence indicators and includes execution identity only for persisted runs

#### Scenario: Validate route exposes compile-check without persistence
- **WHEN** POST /api/workflows/validate is called with a WorkflowDto payload
- **THEN** the API returns a structured validation result with IsValid and Errors and does not write to the persistence store
