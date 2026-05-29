# workflow-version-history Specification

## Purpose
Define how workflow revisions are stored, queried, and activated across a single workflow identity.

## ADDED Requirements

### Requirement: Workflow revisions are retained per workflow identity
The system SHALL persist every workflow revision for a workflow identity and SHALL assign each revision an auto-incrementing integer version number scoped to that workflow identity.

#### Scenario: Create the first revision for a workflow identity
- **WHEN** the client creates a workflow identity with no prior revisions
- **THEN** the system persists version 1 for that workflow identity and marks that revision active

#### Scenario: Create a later revision for the same workflow identity
- **WHEN** the client creates or updates the same workflow identity again
- **THEN** the system stores a new revision with the next integer version number and retains earlier revisions

### Requirement: Only one workflow revision is active at a time
The system SHALL allow at most one active revision per workflow identity and SHALL deactivate the previously active revision when another revision is activated.

#### Scenario: Activate an older revision
- **WHEN** the client activates version 8 for a workflow identity that currently has version 10 active
- **THEN** version 8 becomes active and the other revisions for that workflow identity are no longer active

### Requirement: Workflow version history is queryable
The system SHALL expose the historical revisions for a workflow identity in version order so callers can inspect or restore prior workflow definitions.

#### Scenario: List workflow revisions
- **WHEN** the client requests the version history for a workflow identity
- **THEN** the response includes every retained revision ordered by version number and identifies which revision is currently active