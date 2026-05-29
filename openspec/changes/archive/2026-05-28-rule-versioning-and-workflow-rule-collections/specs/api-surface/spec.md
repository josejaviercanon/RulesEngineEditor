## MODIFIED Requirements

### Requirement: Workflow version management endpoints
The system SHALL expose HTTP endpoints to manage both workflow revisions and rule revisions within a workflow context. The API MUST allow listing rule versions for a `RuleGuidId`, activating a specific retained rule version, and querying workflow rules in active-only, latest-per-rule, or history-inclusive modes while preserving retained revision history.

#### Scenario: List rule revisions for a rule identity
- **WHEN** the client submits a request to list versions for a `RuleGuidId`
- **THEN** the API returns all retained versions in version order and identifies which version is active

#### Scenario: Activate specific rule revision
- **WHEN** the client submits a request to activate version 8 for a `RuleGuidId` that currently has version 10 active
- **THEN** the API marks version 8 active, marks other versions inactive for that `RuleGuidId`, and returns updated active-version metadata

#### Scenario: Query workflow rules in active-only mode
- **WHEN** the client requests workflow rules with active-only mode
- **THEN** the API returns one active rule revision per `RuleGuidId` and does not return duplicate logical rules

#### Scenario: Query workflow rules in latest-per-rule mode
- **WHEN** the client requests workflow rules with latest-per-rule mode
- **THEN** the API returns one highest-version rule revision per `RuleGuidId` regardless of active flag
