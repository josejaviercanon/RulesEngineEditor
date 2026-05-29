## MODIFIED Requirements

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

## ADDED Requirements

### Requirement: Rule edit and rule version-create are separate API intents
The system MUST expose distinct backend intents for updating an existing selected rule version and for creating a new rule version.

#### Scenario: Update endpoint does not create version
- **WHEN** a rule update intent is executed
- **THEN** version count for the target `RuleGuidId` remains unchanged

#### Scenario: Create-version endpoint always creates version
- **WHEN** a rule create-version intent is executed with valid data
- **THEN** version count for the target `RuleGuidId` increases by exactly one
