## ADDED Requirements

### Requirement: Workflow Create Operation
The system SHALL support creating new workflows through a POST operation that persists workflow definition data and returns the created resource identity.

#### Scenario: Create workflow
- **GIVEN** a valid workflow payload with a unique name
- **WHEN** the client submits POST /api/workflows
- **THEN** the workflow is persisted and the response indicates creation with workflow identity metadata

### Requirement: Workflow Read Operation
The system SHALL support retrieving workflow definitions by identifier and listing workflows via GET operations.

#### Scenario: Read workflow by id
- **GIVEN** an existing workflow identifier
- **WHEN** the client submits GET /api/workflows/{id}
- **THEN** the response returns workflow definition and metadata

### Requirement: Workflow Update Operation
The system SHALL support updating an existing workflow through a PUT operation with validation before persistence.

#### Scenario: Update workflow
- **GIVEN** an existing workflow and a valid updated payload
- **WHEN** the client submits PUT /api/workflows/{id}
- **THEN** the stored workflow definition is replaced with the updated content

### Requirement: Workflow Delete Operation
The system SHALL support deleting an existing workflow through a DELETE operation.

#### Scenario: Delete workflow
- **GIVEN** an existing workflow identifier
- **WHEN** the client submits DELETE /api/workflows/{id}
- **THEN** the workflow is removed and the response confirms deletion semantics

### Requirement: Workflow Validation Before Execution
The system SHALL validate workflow definitions against the configured schema version before any execution path is invoked.

#### Scenario: Reject invalid workflow execution request
- **GIVEN** a workflow payload that violates required schema constraints
- **WHEN** an execution request is submitted
- **THEN** the system rejects the request with structured validation errors

### Requirement: Dry-Run Execution Path
The system SHALL support a dry-run mode that evaluates workflows without persisting execution side effects.

#### Scenario: Dry-run execution
- **GIVEN** a valid workflow and test input parameters
- **WHEN** execute is requested with dryRun=true
- **THEN** evaluation results are returned and no execution state is persisted

### Requirement: Real Execution Path
The system SHALL support a real execution mode that evaluates workflows and persists execution state.

#### Scenario: Real execution
- **GIVEN** a valid workflow and production input parameters
- **WHEN** execute is requested with dryRun=false
- **THEN** evaluation results are returned and execution state is persisted
