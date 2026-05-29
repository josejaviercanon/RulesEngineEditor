## ADDED Requirements

### Requirement: Rule status is persisted with constrained lifecycle values
The system SHALL persist a status value for every rule revision using the allowed lifecycle values `draft`, `failed`, `disabled`, and `production`, and SHALL default new rules to `draft` when status is omitted.

#### Scenario: New rule defaults to draft
- **WHEN** a user creates a new rule without providing status
- **THEN** the persisted rule status is `draft`

#### Scenario: Invalid status value is rejected
- **WHEN** a create or update request provides a status value outside `draft|failed|disabled|production`
- **THEN** the request is rejected with a structured validation error and no status change is persisted

### Requirement: User authority governs disabled and production transitions
The system SHALL require explicit user-initiated status change operations to set `disabled` or `production`, and SHALL only allow transition to `production` when all workflow and expression validations pass.

#### Scenario: User can disable a rule regardless of compile result
- **WHEN** a user requests status transition for a rule to `disabled`
- **THEN** the rule status is set to `disabled` and the rule is excluded from execution consideration

#### Scenario: User can set production only after successful validation
- **WHEN** a user requests status transition to `production` for a rule that passes structural and expression validation
- **THEN** the rule status is set to `production`

#### Scenario: Production transition is rejected when validation fails
- **WHEN** a user requests status transition to `production` for a rule with validation or compilation errors
- **THEN** the transition is rejected and the original status remains unchanged

### Requirement: System applies automatic failed transitions on compile and runtime faults
The system SHALL automatically transition rule status to `failed` when compile or runtime failures occur for rules that are not `draft` and not `disabled`, and SHALL preserve status for `draft`, `failed`, and `disabled` according to policy.

#### Scenario: Compile failure transitions production rule to failed
- **WHEN** rule compilation fails for a rule currently in `production`
- **THEN** the rule status is transitioned to `failed`

#### Scenario: Runtime failure transitions active production rule to failed
- **WHEN** an active `production` rule fails during workflow execution
- **THEN** the rule status is transitioned to `failed`

#### Scenario: Runtime failure keeps draft rule in draft
- **WHEN** a `draft` rule fails during dry-run or execution
- **THEN** the rule remains `draft` and the failure is reported in results

#### Scenario: Runtime failure keeps failed rule in failed
- **WHEN** a `failed` rule fails again during dry-run or execution
- **THEN** the rule remains `failed` and the failure is reported in results

#### Scenario: Disabled rules are not auto-transitioned
- **WHEN** a rule is `disabled`
- **THEN** the rule is excluded from execution and no system-initiated transition is applied
