## MODIFIED Requirements

### Requirement: Workflow revisions are retained per workflow identity
The system SHALL persist every workflow revision for a workflow identity and SHALL assign each revision an auto-incrementing integer version number scoped to that workflow identity. The system MUST create new revisions only when an explicit create-new-version operation is requested.

#### Scenario: Create the first revision for a workflow identity
- **WHEN** the client creates a workflow identity with no prior revisions
- **THEN** the system persists version 1 for that workflow identity and marks that revision active

#### Scenario: Create a later revision for the same workflow identity
- **WHEN** the client executes an explicit create-new-version operation for an existing workflow identity
- **THEN** the system stores a new revision with the next integer version number and retains earlier revisions

#### Scenario: Edit-save updates selected workflow version in place
- **WHEN** the client executes a standard edit-save operation for a selected workflow version
- **THEN** the system updates only that selected persisted version
- **AND** no additional workflow revision is created

## ADDED Requirements

### Requirement: Workflow list projections include active version metadata
The system SHALL include active-version metadata in workflow list projections used by Home and Rules page grids.

#### Scenario: List response includes active version field
- **WHEN** the client requests workflow list data
- **THEN** each workflow row includes active version metadata that can be rendered independently from latest-version count
