# workflow-rule-collections Specification

## Purpose
Define workflow-level persistence and retrieval behavior for logical rule memberships and versioned rule projections.

## Requirements
### Requirement: Workflow stores a collection of rule Guid identities
The system SHALL persist workflow membership as a collection of logical rule identities (`RuleGuidId`) so workflow rule retrieval is resolved from an authoritative membership set.

#### Scenario: Persist workflow with rule collection
- **WHEN** a workflow is created or updated with a set of rules
- **THEN** the system stores workflow-to-`RuleGuidId` associations representing that workflow's rule collection

### Requirement: Workflow active-rule query returns one active revision per rule identity
The system SHALL provide a query mode that returns only active revisions for each `RuleGuidId` in a workflow and MUST avoid duplicate logical rules.

#### Scenario: Query active workflow rules
- **WHEN** the client requests active rules for a workflow
- **THEN** the response includes at most one rule revision per `RuleGuidId` and each returned revision is active

### Requirement: Workflow latest-rule query returns latest revision per rule identity
The system SHALL provide a query mode that returns the latest version per `RuleGuidId` for a workflow regardless of active flag and MUST avoid duplicate logical rules.

#### Scenario: Query latest workflow rules
- **WHEN** the client requests latest rules for a workflow including inactive latest revisions
- **THEN** the response returns exactly one revision per `RuleGuidId` with the highest version number

### Requirement: Workflow full-history query is explicit
The system SHALL return all revisions for all workflow rule identities only when the caller explicitly requests history-inclusive mode.

#### Scenario: Query workflow full history
- **WHEN** the client requests workflow rules with history mode enabled
- **THEN** the response includes all versions for each `RuleGuidId` in that workflow and identifies active revision state
