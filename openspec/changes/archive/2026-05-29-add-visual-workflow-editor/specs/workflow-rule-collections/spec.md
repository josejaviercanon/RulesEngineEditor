## ADDED Requirements

### Requirement: Workflow rules include explicit execute order
The system SHALL persist an `ExecuteOrder` numeric field for each rule in a workflow context and MUST use it as the canonical ordering key for workflow rule execution sequence displays and save-time ordering normalization.

#### Scenario: Persist and return execute order
- **WHEN** a rule is created or updated with `ExecuteOrder`
- **THEN** the persisted rule revision stores that numeric value
- **AND** workflow rule retrieval APIs return `ExecuteOrder` for each rule

#### Scenario: Rules detail grid is ordered by execute order
- **WHEN** the Rules page nested detail grid loads rules for a workflow
- **THEN** rows are shown in ascending `ExecuteOrder`
- **AND** ties are resolved deterministically by rule identity/version

### Requirement: Visual links and execute order remain consistent on save
The system SHALL normalize diagram link ordering and rule `ExecuteOrder` values to a deterministic sequence during save operations.

#### Scenario: Save normalizes mixed node order edits
- **WHEN** a user reorders nodes visually and edits `ExecuteOrder` values
- **THEN** the save operation produces a deterministic ordered rule sequence
- **AND** persisted workflow/rule representations reflect that sequence consistently
