# rule-versioning-and-activation Specification

## Purpose
Define rule-level revision history, activation behavior, and invariants for stable rule identities.

## Requirements
### Requirement: Rule revisions are retained per rule Guid identity
The system SHALL persist every rule revision under a stable `RuleGuidId` and SHALL assign each revision an auto-incrementing integer `Version` value scoped to that `RuleGuidId`.

#### Scenario: Create first rule revision
- **WHEN** a new rule Guid identity is created with no prior revisions
- **THEN** the system stores revision version 1 for that `RuleGuidId` and marks it active

#### Scenario: Create later rule revision
- **WHEN** the same `RuleGuidId` is updated with a new rule definition
- **THEN** the system stores a new revision with `Version = previous max + 1` and retains all earlier revisions

### Requirement: Exactly one rule revision is active per rule Guid identity
The system SHALL allow only one active revision for a given `RuleGuidId` at any time and MUST enforce this invariant in persistence and service operations.

#### Scenario: Activate an older revision
- **WHEN** version 8 is activated for a `RuleGuidId` that currently has version 10 active
- **THEN** version 8 becomes active and versions 9 and 10 become inactive for that `RuleGuidId`

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
The system SHALL provide rule list responses for a workflow context that include stable identity and version metadata required by the nested Rules page grid.

#### Scenario: Nested rules grid can render required columns
- **WHEN** the UI requests rules for a selected workflow in details view
- **THEN** each returned rule row includes Guid identity, name, and version metadata used by the grid
