## ADDED Requirements

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
