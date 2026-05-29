## ADDED Requirements

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
