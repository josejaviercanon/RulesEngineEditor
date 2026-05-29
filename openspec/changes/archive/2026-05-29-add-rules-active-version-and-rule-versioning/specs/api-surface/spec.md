## MODIFIED Requirements

### Requirement: Workflow version management endpoints
The system SHALL expose HTTP endpoints to manage both workflow revisions and rule revisions within a workflow context. The API MUST allow listing rule versions for a `RuleGuidId`, activating a specific retained rule version, and querying workflow rules in active-only, latest-per-rule, or history-inclusive modes while preserving retained revision history.

#### Scenario: List rule revisions for a rule identity
- **WHEN** the client submits a request to list versions for a `RuleGuidId`
- **THEN** the API returns all retained versions in version order and identifies which version is active

#### Scenario: Rule version payload supports modal version switching
- **WHEN** the client requests a specific selected rule version from the versions list workflow
- **THEN** the API response provides expression and all persisted rule properties needed to repopulate the edit modal for that selected version
- **AND** the response includes active-state metadata for the selected version

#### Scenario: Activate specific rule revision
- **WHEN** the client submits a request to activate version 8 for a `RuleGuidId` that currently has version 10 active
- **THEN** the API marks version 8 active, marks other versions inactive for that `RuleGuidId`, and returns updated active-version metadata

#### Scenario: Activation save deactivates old active before applying new active
- **WHEN** the client saves a rule edit that changes active version
- **THEN** the API performs activation as one invariant-preserving operation where previous active revision is no longer active after update completion
- **AND** only one active revision remains for the target `RuleGuidId`

#### Scenario: Query workflow rules in active-only mode
- **WHEN** the client requests workflow rules with active-only mode
- **THEN** the API returns one active rule revision per `RuleGuidId` and does not return duplicate logical rules

#### Scenario: Query workflow rules in latest-per-rule mode
- **WHEN** the client requests workflow rules with latest-per-rule mode
- **THEN** the API returns one highest-version rule revision per `RuleGuidId` regardless of active flag

### Requirement: API supports UI workflow management actions for Home and Rules pages
The API SHALL provide contracts that allow the UI to load one-row-per-workflow listings, list workflow versions, activate selected workflow versions, and apply workflow enable/disable transitions with consistent validation responses. Rules page contracts SHALL include distinct active-version and last-version metadata.

#### Scenario: Home workflow list is loadable from API
- **WHEN** the UI requests workflow list data for Home or Rules pages
- **THEN** the API returns workflow entries that include Guid identity, active version metadata, workflow name, and enablement state required by the grid

#### Scenario: Rules page workflow list provides distinct version columns
- **WHEN** the Rules page requests workflow list data
- **THEN** each row includes separate active version and last version fields
- **AND** active version does not require client-side derivation from highest retained version

#### Scenario: Rules page nested details return active-only rule rows
- **WHEN** the UI requests rules for an expanded workflow row on Rules page
- **THEN** the API returns active-only rule rows with one row per `RuleGuidId`
- **AND** each row includes Guid ID, Name, Active Version, and Last Version metadata

#### Scenario: Workflow version list and activation are available for edit modal
- **WHEN** the UI requests versions for a workflow identity and activates a selected version
- **THEN** the API returns version history and applies activation so only one version remains active for that workflow identity

#### Scenario: Enable or disable returns actionable validation errors
- **WHEN** the UI requests enable/disable for a workflow version that violates lifecycle constraints
- **THEN** the API returns a structured validation response suitable for modal feedback
