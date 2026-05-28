## MODIFIED Requirements

### Requirement: Workflow Create Operation
The system SHALL support creating new workflows through a POST operation that persists workflow definition data, validates schema compatibility, and returns the created resource identity with persisted lifecycle metadata.

#### Scenario: Create workflow with validated definition
- **WHEN** the client submits POST /api/workflows with a valid workflow payload
- **THEN** the workflow is persisted and the response indicates creation with workflow identity and lifecycle metadata

### Requirement: Workflow Update Operation
The system SHALL support updating an existing workflow through a PUT operation with schema validation and deterministic replacement semantics for workflow definition content.

#### Scenario: Update workflow with valid payload
- **WHEN** the client submits PUT /api/workflows/{id} with a valid updated payload
- **THEN** the stored workflow definition is replaced and returned using the canonical response contract

### Requirement: Workflow Validation Before Execution
The system SHALL validate workflow definitions against the configured schema version before any execution path is invoked and SHALL return structured validation failures without persisting execution state.

#### Scenario: Reject invalid workflow execution request
- **WHEN** an execution request is submitted for a workflow definition that violates required schema constraints
- **THEN** the system rejects the request with structured validation errors and does not execute rules

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
