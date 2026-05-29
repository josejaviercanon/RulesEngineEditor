## MODIFIED Requirements

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
