# rule-versioning-and-activation Specification

## Purpose
Define rule-level revision history, activation behavior, and invariants for stable rule identities.
## Requirements
### Requirement: Rule revisions are retained per rule Guid identity
The system SHALL persist rule revisions under a stable `RuleGuidId`, SHALL assign each new revision an auto-incrementing integer `Version` scoped to that `RuleGuidId`, and MUST create a new revision only for explicit create-version operations.

#### Scenario: Create first rule revision
- **WHEN** a new rule Guid identity is created with no prior revisions
- **THEN** the system stores revision version 1 for that `RuleGuidId` and marks it active

#### Scenario: Create later rule revision from explicit new-version action
- **WHEN** the client submits an explicit create-new-version operation for an existing `RuleGuidId`
- **THEN** the system stores a new revision with `Version = previous max + 1`
- **AND** earlier revisions are retained

#### Scenario: Edit-save updates selected version in place
- **WHEN** the client submits a standard edit-save operation for a selected rule version
- **THEN** the system updates only that selected persisted version
- **AND** no additional rule revision is created

### Requirement: Exactly one rule revision is active per rule Guid identity
The system SHALL allow only one active revision for a given `RuleGuidId` at any time and MUST enforce this invariant in persistence and service operations, including activation changes submitted from rule edit saves.

#### Scenario: Activate an older revision
- **WHEN** version 8 is activated for a `RuleGuidId` that currently has version 10 active
- **THEN** version 8 becomes active and versions 9 and 10 become inactive for that `RuleGuidId`

#### Scenario: Rule edit save switches active version deterministically
- **WHEN** an edit-save request sets version 8 active for a `RuleGuidId` that currently has version 10 active
- **THEN** the system deactivates version 10 before completing activation of version 8
- **AND** exactly one active version remains for that `RuleGuidId` after persistence

#### Scenario: Concurrent activation requests
- **WHEN** two activation operations for different versions of the same `RuleGuidId` race concurrently
- **THEN** exactly one requested version remains active after both operations complete

### Requirement: Activation does not create a new revision
The system SHALL switch active state on existing revisions and MUST NOT create a new revision row when activating a previously stored version.

#### Scenario: Re-activate historical version without cloning
- **WHEN** an operator activates historical version 3 for a `RuleGuidId`
- **THEN** the system only updates active flags and version history count remains unchanged

### Requirement: Rule edit flows enforce expression validation before persisting revisions
The system SHALL validate rule expression syntax/semantics for rule create and update flows and MUST reject invalid expressions before persisting a new or updated rule revision.

#### Scenario: Create rule with invalid expression is rejected
- **WHEN** a rule create request contains an invalid expression
- **THEN** the request is rejected with validation details
- **AND** no new rule revision is persisted

#### Scenario: Update rule with invalid expression is rejected
- **WHEN** a rule update request contains an invalid expression
- **THEN** the request is rejected with validation details
- **AND** existing persisted revisions remain unchanged

### Requirement: Rule list views expose version metadata for expandable workflow details
The system SHALL provide rule list responses for a workflow context that include stable identity and version metadata required by the nested Rules page grid, including active-version and last-version values rendered as separate columns.

#### Scenario: Nested rules grid can render required columns
- **WHEN** the UI requests rules for a selected workflow in details view
- **THEN** each returned rule row includes Guid identity, name, active version metadata, and last version metadata used by the grid

#### Scenario: Details grid includes only active rule revisions
- **WHEN** the UI requests rules for a selected workflow in active-only display mode
- **THEN** the response contains only active revisions
- **AND** the response includes at most one row per `RuleGuidId`

### Requirement: Rule edit and rule version-create are separate API intents
The system MUST expose distinct backend intents for updating an existing selected rule version and for creating a new rule version.

#### Scenario: Update endpoint does not create version
- **WHEN** a rule update intent is executed
- **THEN** version count for the target `RuleGuidId` remains unchanged

#### Scenario: Create-version endpoint always creates version
- **WHEN** a rule create-version intent is executed with valid data
- **THEN** version count for the target `RuleGuidId` increases by exactly one

