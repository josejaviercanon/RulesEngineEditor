## MODIFIED Requirements

### Requirement: Workflow CRUD Endpoints
The system SHALL expose HTTP endpoints for workflow lifecycle operations and SHALL enforce consistent request validation, response payloads, and error contracts across create, read, update, delete, validate, and execute routes.

#### Scenario: CRUD operations use consistent contracts
- **WHEN** requests are made to /api/workflows for create, read, update, or delete operations
- **THEN** responses include stable workflow metadata fields and HTTP status semantics aligned to operation outcome

#### Scenario: Validation errors are structurally consistent
- **WHEN** a workflow request fails schema or contract validation on any workflow lifecycle endpoint
- **THEN** the API returns a structured error payload containing schema version context and an errors collection

#### Scenario: Execute route distinguishes dry-run and persisted execution
- **WHEN** POST /api/workflows/{id}/execute is called with dryRun=true or dryRun=false
- **THEN** the API returns execution results with explicit dry-run and persistence indicators and includes execution identity only for persisted runs
