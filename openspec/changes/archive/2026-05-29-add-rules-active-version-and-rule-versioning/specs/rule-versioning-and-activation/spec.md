## MODIFIED Requirements

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

### Requirement: Rule list views expose version metadata for expandable workflow details
The system SHALL provide rule list responses for a workflow context that include stable identity and version metadata required by the nested Rules page grid, including active-version and last-version values rendered as separate columns.

#### Scenario: Nested rules grid can render required columns
- **WHEN** the UI requests rules for a selected workflow in details view
- **THEN** each returned rule row includes Guid identity, name, active version metadata, and last version metadata used by the grid

#### Scenario: Details grid includes only active rule revisions
- **WHEN** the UI requests rules for a selected workflow in active-only display mode
- **THEN** the response contains only active revisions
- **AND** the response includes at most one row per `RuleGuidId`
