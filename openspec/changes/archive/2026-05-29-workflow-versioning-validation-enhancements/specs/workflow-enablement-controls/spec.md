## ADDED Requirements

### Requirement: New workflow creation defaults to disabled
The system SHALL default newly created workflow revisions to `IsEnabled=false` when no explicit enablement value is provided by the client.

#### Scenario: Create workflow without explicit enablement
- **WHEN** a client creates a new workflow and omits an explicit enablement override
- **THEN** the persisted workflow revision has `IsEnabled=false`

#### Scenario: UI create modal initializes enable toggle as off
- **WHEN** a user opens the New Workflow modal
- **THEN** the Enable toggle initial value is OFF
