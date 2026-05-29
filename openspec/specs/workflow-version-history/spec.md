# workflow-version-history Specification

## Purpose
Define how workflow revisions are stored, queried, and activated across a single workflow identity.
## Requirements
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

### Requirement: Only one workflow revision is active at a time
The system SHALL allow at most one active revision per workflow identity and SHALL deactivate the previously active revision when another revision is activated.

#### Scenario: Activate an older revision
- **WHEN** the client activates version 8 for a workflow identity that currently has version 10 active
- **THEN** version 8 becomes active and the other revisions for that workflow identity are no longer active

### Requirement: Workflow version history is queryable
The system SHALL expose workflow revision history and related rule revision history so callers can inspect retained workflow definitions and the retained versions of rules associated with each workflow identity. The system MUST support retrieval modes that return active-only or latest-only rule revisions per rule identity to avoid duplicate logical rules in standard workflow responses.

#### Scenario: List workflow revisions with duplicate-free active rules
- **WHEN** the client requests version history for a workflow identity in active-only rule mode
- **THEN** each workflow revision response contains at most one active rule revision per `RuleGuidId`

#### Scenario: List workflow revisions with duplicate-free latest rules
- **WHEN** the client requests version history for a workflow identity in latest-per-rule mode
- **THEN** each workflow revision response contains at most one highest-version rule revision per `RuleGuidId` even if that revision is inactive

#### Scenario: List workflow revisions with full rule history
- **WHEN** the client requests version history for a workflow identity with history-inclusive rule mode
- **THEN** the response includes all retained rule revisions for each `RuleGuidId` and identifies the active rule revision for each identity

### Requirement: Workflow list projections include active version metadata
The system SHALL include active-version metadata in workflow list projections used by Home and Rules page grids.

#### Scenario: List response includes active version field
- **WHEN** the client requests workflow list data
- **THEN** each workflow row includes active version metadata that can be rendered independently from latest-version count

