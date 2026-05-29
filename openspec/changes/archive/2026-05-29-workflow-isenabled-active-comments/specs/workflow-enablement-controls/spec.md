## ADDED Requirements

### Requirement: Active workflow version supports explicit enablement state
The system SHALL persist an `IsEnabled` state on workflow revisions and SHALL allow enable/disable transitions only for the currently active revision of a workflow identity.

#### Scenario: Enable active workflow revision
- **WHEN** the client requests enabling the active revision for a workflow identity
- **THEN** the system sets that revision `IsEnabled=true` and persists the state change

#### Scenario: Disable active workflow revision
- **WHEN** the client requests disabling the active revision for a workflow identity
- **THEN** the system sets that revision `IsEnabled=false` and persists the state change

#### Scenario: Reject enable/disable on non-active revision
- **WHEN** the client requests enabling or disabling a workflow revision that is not active for that workflow identity
- **THEN** the system rejects the request with a validation error and does not change persisted state

### Requirement: Only one revision per workflow identity can be active and enabled
The system MUST enforce that at most one workflow revision per workflow identity is simultaneously `IsActive=true` and `IsEnabled=true`.

#### Scenario: Enabling active revision while another revision is active
- **WHEN** the active revision for a workflow identity is enabled
- **THEN** no other revision for that same workflow identity is persisted with both `IsActive=true` and `IsEnabled=true`

#### Scenario: Concurrent enable operations
- **WHEN** concurrent operations attempt to create more than one active+enabled revision for the same workflow identity
- **THEN** the system resolves the race so the invariant remains true after both operations complete

### Requirement: Workflow list APIs support nullable enablement filtering
The system SHALL support workflow listing/filtering by nullable enablement parameter with three explicit modes: enabled-only, disabled-only, and unfiltered.

#### Scenario: Filter enabled workflows
- **WHEN** the client requests workflow listing with enablement filter set to true
- **THEN** the response includes only workflow revisions with `IsEnabled=true` according to endpoint semantics

#### Scenario: Filter disabled workflows
- **WHEN** the client requests workflow listing with enablement filter set to false
- **THEN** the response includes only workflow revisions with `IsEnabled=false` according to endpoint semantics

#### Scenario: Return all workflows when filter not provided
- **WHEN** the client omits the enablement filter parameter or provides null
- **THEN** the response includes both enabled and disabled workflows according to endpoint semantics
