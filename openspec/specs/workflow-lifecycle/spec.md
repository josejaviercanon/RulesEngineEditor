# workflow-lifecycle Specification

## Purpose
Define workflow lifecycle behavior for create, read, update, delete, validation gates, execution modes, and persistence preconditions.
## Requirements
### Requirement: Workflow Create Operation
The system SHALL support creating new workflows through a POST operation that persists a first workflow revision using the full typed `WorkflowDto` model (not a raw JSON string), validates schema compatibility, assigns version 1 for a new workflow identity, marks the created revision active, and returns the created resource identity with persisted lifecycle metadata.

#### Scenario: Create workflow with validated full model definition
- **WHEN** the client submits POST /api/workflows with a valid `WorkflowDto` payload including `WorkflowName`, `Rules`, and optionally `GlobalParams`
- **THEN** the workflow is persisted as version 1 for that workflow identity, the created revision is marked active, and the response indicates creation with workflow identity and lifecycle metadata

### Requirement: Workflow Read Operation
The system SHALL support retrieving the currently active workflow revision by identifier, listing workflow revisions, and retrieving a specific retained revision by version via GET operations. Workflow read responses MUST include lifecycle metadata including activation and enablement state.

#### Scenario: Read workflow by id
- **WHEN** the client submits GET /api/workflows/{id}
- **THEN** the response returns the currently active workflow revision, including lifecycle metadata with `IsActive` and `IsEnabled`

#### Scenario: Read workflow revision history
- **WHEN** the client submits GET /api/workflows/{id}/versions
- **THEN** the response returns retained revisions for that workflow identity in version order with lifecycle metadata for each revision

#### Scenario: Read workflow by id and explicit version
- **WHEN** the client submits GET /api/workflows/{id}/versions/{version}
- **THEN** the response returns that retained revision when it exists, including `IsActive` and `IsEnabled`, otherwise not-found

### Requirement: Workflow Update Operation
The system SHALL support updating an existing workflow through a PUT operation using the full typed `WorkflowDto` model with schema validation and deterministic versioning semantics for workflow definition content.

#### Scenario: Update workflow with valid full model payload
- **WHEN** the client submits PUT /api/workflows/{id} with a valid updated `WorkflowDto` payload
- **THEN** the stored workflow definition is persisted as a new version for that workflow identity, the prior revisions remain available, and the canonical response contract returns the new active revision with version metadata

### Requirement: Workflow Delete Operation
The system SHALL support deleting an existing workflow through a DELETE operation.

#### Scenario: Delete workflow
- **WHEN** the client submits DELETE /api/workflows/{id}
- **THEN** the workflow identity and its retained revisions are removed and the response confirms deletion semantics

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
The system SHALL support a dry-run mode that evaluates workflows without persisting execution side effects and SHALL return explicit dry-run metadata to callers. Dry-run evaluation SHALL apply active-version and status filters, include `draft`, `failed`, and `production` by default, and exclude `disabled`.

#### Scenario: Dry-run execution
- **WHEN** execute is requested with dryRun=true
- **THEN** evaluation results are returned and no execution state is persisted

#### Scenario: Dry-run default status inclusion excludes disabled
- **WHEN** execute is requested with dryRun=true and no status filter
- **THEN** only active rules in `draft`, `failed`, and `production` are evaluated and `disabled` rules are skipped

#### Scenario: Dry-run failure preserves draft and failed statuses
- **WHEN** execute is requested with dryRun=true and evaluated `draft` or `failed` rules fail
- **THEN** failures are reported and persisted status remains unchanged

### Requirement: Real Execution Path
The system SHALL support a real execution mode that evaluates workflows, persists execution state, and returns an execution identifier for result traceability. During real execution, if an active rule in `production` status fails evaluation or action execution, the system SHALL persist a transition of that rule status to `failed`; rules in `draft` SHALL remain `draft`, rules in `failed` SHALL remain `failed`, and rules in `disabled` SHALL remain excluded.

#### Scenario: Real execution
- **WHEN** execute is requested with dryRun=false
- **THEN** evaluation results are returned and execution state is persisted with a non-null execution identifier

#### Scenario: Real execution failure transitions production to failed
- **WHEN** execute is requested with dryRun=false and an active `production` rule fails
- **THEN** the rule status is persisted as `failed` and the response reports the transition

#### Scenario: Real execution failure keeps draft in draft
- **WHEN** execute is requested with dryRun=false and an active `draft` rule fails
- **THEN** the failure is reported and the rule status remains `draft`

#### Scenario: Real execution failure keeps failed in failed
- **WHEN** execute is requested with dryRun=false and an active `failed` rule fails
- **THEN** the failure is reported and the rule status remains `failed`

### Requirement: Workflow List includes active version metadata
The system SHALL return the active workflow revision for each workflow identity when listing workflows via GET /api/workflows, including the current version number, active-state metadata, and enablement metadata. The list operation SHALL support nullable enablement filtering.

#### Scenario: List workflows returns active revision per identity
- **WHEN** the client submits GET /api/workflows
- **THEN** each workflow entry in the response includes the active revision for that identity with its full `WorkflowDto` structure and lifecycle metadata including `IsEnabled`

#### Scenario: List workflows returns enabled-only entries
- **WHEN** the client submits GET /api/workflows?isEnabled=true
- **THEN** the response includes only active workflow revisions with `IsEnabled=true`

#### Scenario: List workflows returns disabled-only entries
- **WHEN** the client submits GET /api/workflows?isEnabled=false
- **THEN** the response includes only active workflow revisions with `IsEnabled=false`

#### Scenario: List workflows returns all entries when filter is null or omitted
- **WHEN** the client submits GET /api/workflows without `isEnabled` or with null value
- **THEN** the response includes both enabled and disabled active workflow revisions

