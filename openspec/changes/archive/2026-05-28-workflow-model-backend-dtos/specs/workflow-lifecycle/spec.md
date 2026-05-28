## MODIFIED Requirements

### Requirement: Workflow Create Operation
The system SHALL support creating new workflows through a POST operation that persists workflow definition data using the full typed `WorkflowDto` model (not a raw JSON string), validates schema compatibility, and returns the created resource identity with persisted lifecycle metadata.

#### Scenario: Create workflow with validated full model definition
- **WHEN** the client submits POST /api/workflows with a valid `WorkflowDto` payload including `WorkflowName`, `Rules`, and optionally `GlobalParams`
- **THEN** the workflow is persisted (serialized to the internal JSON store) and the response indicates creation with workflow identity and lifecycle metadata

### Requirement: Workflow Update Operation
The system SHALL support updating an existing workflow through a PUT operation using the full typed `WorkflowDto` model with schema validation and deterministic replacement semantics for workflow definition content.

#### Scenario: Update workflow with valid full model payload
- **WHEN** the client submits PUT /api/workflows/{id} with a valid updated `WorkflowDto` payload
- **THEN** the stored workflow definition is replaced and returned using the canonical response contract with the full typed model

## ADDED Requirements

### Requirement: Workflow List includes full model in response
The system SHALL return the full typed `WorkflowDto` for each workflow when listing workflows via GET /api/workflows, including all rules and global parameters.

#### Scenario: List workflows returns full model per entry
- **WHEN** the client submits GET /api/workflows
- **THEN** each workflow entry in the response includes the full `WorkflowDto` structure with `Rules` and `GlobalParams` populated
