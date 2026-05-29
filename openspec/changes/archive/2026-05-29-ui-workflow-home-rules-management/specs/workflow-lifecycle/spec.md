## ADDED Requirements

### Requirement: Workflow edit operations preserve active and enabled invariants
The system SHALL enforce workflow lifecycle invariants during workflow edit interactions so that exactly one revision per workflow identity remains active and enablement transitions remain consistent with active-state rules.

#### Scenario: Activating a version deactivates other revisions for the same identity
- **WHEN** a user activates a selected workflow version from the edit flow
- **THEN** the selected version is marked active
- **AND** all other versions for that workflow identity are marked inactive

#### Scenario: Enablement updates do not violate active-version lifecycle constraints
- **WHEN** a user updates workflow enablement from the edit flow
- **THEN** the resulting persisted state preserves lifecycle constraints for active and enabled metadata
- **AND** invalid transitions are rejected with validation feedback

#### Scenario: Workflow list reflects post-edit lifecycle state
- **WHEN** a workflow edit operation succeeds
- **THEN** subsequent workflow list reads return updated active version and enablement metadata for that workflow identity
