## MODIFIED Requirements

### Requirement: Workflow version history is queryable
The system SHALL expose workflow revision history and related rule revision history so callers can inspect retained workflow definitions and the retained versions of rules associated with each workflow identity. The system MUST support retrieval modes that return active-only or latest-only rule revisions per rule identity to avoid duplicate logical rules in standard workflow responses.

#### Scenario: List workflow revisions with duplicate-free active rules
- **WHEN** the client requests version history for a workflow identity in active-only rule mode
- **THEN** each workflow revision response contains at most one active rule revision per `RuleGuidId`

#### Scenario: List workflow revisions with duplicate-free latest rules
- **WHEN** the client requests version history for a workflow identity in latest-per-rule mode
- **THEN** each workflow revision response contains at most one highest-version rule revision per `RuleGuidId` even if that revision is inactive

#### Scenario: List workflow revisions with full rule history
- **WHEN** the client requests version history for a workflow identity with history-inclusive rule mode
- **THEN** the response includes all retained rule revisions for each `RuleGuidId` and identifies the active rule revision for each identity
