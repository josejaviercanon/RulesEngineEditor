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

### Requirement: New workflow version initializes with exactly one default rule
The system MUST initialize every newly created workflow version with exactly one rule named `Default Rule` and MUST NOT create additional implicit rules.

#### Scenario: Create new workflow version seeds one default rule
- **WHEN** a new workflow version is created
- **THEN** the workflow version contains exactly one seeded rule named `Default Rule`
- **AND** no additional auto-generated rules are created

### Requirement: Workflow rule collections support explicit rule deletion
The system SHALL allow deleting a selected rule from a workflow rule collection through an explicit delete operation.

#### Scenario: Confirmed delete removes selected rule
- **WHEN** the client confirms deletion for a selected rule in a workflow context
- **THEN** the selected rule is removed from that workflow rule collection
- **AND** subsequent workflow rule list queries do not return the deleted rule

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

