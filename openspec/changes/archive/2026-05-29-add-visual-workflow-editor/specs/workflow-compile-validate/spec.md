## ADDED Requirements

### Requirement: Save workflow operation supports validation warning decision flow
The system SHALL perform dry-run workflow validation before persisting visual editor saves and MUST return a warning decision contract when validation errors are present.

#### Scenario: Save pre-check returns warning decision options
- **WHEN** a save request fails dry-run validation
- **THEN** the response includes validation errors and explicit decision options for `save-as-draft` or `cancel`

#### Scenario: Cancel after warning does not persist
- **WHEN** the client receives a warning decision response and chooses cancel
- **THEN** no workflow or rule persistence occurs

### Requirement: Draft override save preserves validation diagnostics
When a client selects `save-as-draft` after validation warnings, the system SHALL persist the workflow and SHALL return validation diagnostics to support user remediation.

#### Scenario: Save as draft returns persisted result with warnings
- **WHEN** save-as-draft is requested after validation warnings
- **THEN** the workflow is persisted as draft-compatible state
- **AND** the response includes warning/validation details for UI display
