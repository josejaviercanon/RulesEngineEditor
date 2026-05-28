# workflow-lifecycle Specification

## Purpose
Define workflow lifecycle behavior for create, read, update, delete, validation gates, execution modes, and persistence preconditions.
## Requirements
### Requirement: Workflow Create Operation
The system SHALL support creating new workflows through a POST operation that persists workflow definition data using the full typed `WorkflowDto` model (not a raw JSON string), validates schema compatibility, and returns the created resource identity with persisted lifecycle metadata.

#### Scenario: Create workflow with validated full model definition
- **WHEN** the client submits POST /api/workflows with a valid `WorkflowDto` payload including `WorkflowName`, `Rules`, and optionally `GlobalParams`
- **THEN** the workflow is persisted (serialized to the internal JSON store) and the response indicates creation with workflow identity and lifecycle metadata

### Requirement: Workflow Read Operation
The system SHALL support retrieving workflow definitions by identifier and listing workflows via GET operations.

#### Scenario: Read workflow by id
- **WHEN** the client submits GET /api/workflows/{id}
- **THEN** the response returns workflow definition and metadata

### Requirement: Workflow Update Operation
The system SHALL support updating an existing workflow through a PUT operation using the full typed `WorkflowDto` model with schema validation and deterministic replacement semantics for workflow definition content.

#### Scenario: Update workflow with valid full model payload
- **WHEN** the client submits PUT /api/workflows/{id} with a valid updated `WorkflowDto` payload
- **THEN** the stored workflow definition is replaced and returned using the canonical response contract with the full typed model

### Requirement: Workflow Delete Operation
The system SHALL support deleting an existing workflow through a DELETE operation.

#### Scenario: Delete workflow
- **WHEN** the client submits DELETE /api/workflows/{id}
- **THEN** the workflow is removed and the response confirms deletion semantics

### Requirement: Workflow Validation Before Execution
The system SHALL validate workflow definitions against the configured schema version before any execution path is invoked and SHALL return structured validation failures without persisting execution state.

#### Scenario: Reject invalid workflow execution request
- **WHEN** an execution request is submitted for a workflow definition that violates required schema constraints
- **THEN** the system rejects the request with structured validation errors and does not execute rules

### Requirement: Workflow Persistence Connection Precondition
The system SHALL require PostgreSQL 18 connectivity to be configured before workflow lifecycle create or update operations are processed.

#### Scenario: Reject persistence operations when database connection is not configured
- **WHEN** create or update workflow operations are invoked
- **THEN** the system returns structured persistence configuration errors and does not write to rules storage

### Requirement: Dry-Run Execution Path
The system SHALL support a dry-run mode that evaluates workflows without persisting execution side effects and SHALL return explicit dry-run metadata to callers.

#### Scenario: Dry-run execution
- **WHEN** execute is requested with dryRun=true
- **THEN** evaluation results are returned and no execution state is persisted

### Requirement: Real Execution Path
The system SHALL support a real execution mode that evaluates workflows, persists execution state, and returns an execution identifier for result traceability.

#### Scenario: Real execution
- **WHEN** execute is requested with dryRun=false
- **THEN** evaluation results are returned and execution state is persisted with a non-null execution identifier

### Requirement: Workflow List includes full model in response
The system SHALL return the full typed `WorkflowDto` for each workflow when listing workflows via GET /api/workflows, including all rules and global parameters.

#### Scenario: List workflows returns full model per entry
- **WHEN** the client submits GET /api/workflows
- **THEN** each workflow entry in the response includes the full `WorkflowDto` structure with `Rules` and `GlobalParams` populated

