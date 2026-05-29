## ADDED Requirements

### Requirement: API supports UI workflow management actions for Home and Rules pages
The API SHALL provide contracts that allow the UI to load one-row-per-workflow listings, list workflow versions, activate selected workflow versions, and apply workflow enable/disable transitions with consistent validation responses.

#### Scenario: Home workflow list is loadable from API
- **WHEN** the UI requests workflow list data for Home or Rules pages
- **THEN** the API returns workflow entries that include Guid identity, active version metadata, workflow name, and enablement state required by the grid

#### Scenario: Workflow version list and activation are available for edit modal
- **WHEN** the UI requests versions for a workflow identity and activates a selected version
- **THEN** the API returns version history and applies activation so only one version remains active for that workflow identity

#### Scenario: Enable or disable returns actionable validation errors
- **WHEN** the UI requests enable/disable for a workflow version that violates lifecycle constraints
- **THEN** the API returns a structured validation response suitable for modal feedback

### Requirement: API returns rule expression validation feedback on save attempts
The API SHALL validate rule expression content on rule create/update operations and SHALL return user-consumable validation details when validation fails.

#### Scenario: Rule expression validation failure on create
- **WHEN** the UI submits a new rule with an invalid expression
- **THEN** the API rejects the request with structured validation messages describing expression errors

#### Scenario: Rule expression validation failure on update
- **WHEN** the UI submits an edit for an existing rule with an invalid expression
- **THEN** the API rejects the request with structured validation messages without mutating persisted rule data
