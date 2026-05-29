## MODIFIED Requirements

### Requirement: Dry-Run Execution Path
The system SHALL support a dry-run mode that evaluates workflows without persisting execution side effects and SHALL return explicit dry-run metadata to callers. Dry-run evaluation SHALL apply active-version and status filters, include `draft`, `failed`, and `production` by default, and exclude `disabled`.

#### Scenario: Dry-run execution
- **WHEN** execute is requested with dryRun=true
- **THEN** evaluation results are returned and no execution state is persisted

#### Scenario: Dry-run default status inclusion excludes disabled
- **WHEN** execute is requested with dryRun=true and no status filter
- **THEN** only active rules in `draft`, `failed`, and `production` are evaluated and `disabled` rules are skipped

#### Scenario: Dry-run failure preserves draft and failed statuses
- **WHEN** execute is requested with dryRun=true and evaluated `draft` or `failed` rules fail
- **THEN** failures are reported and persisted status remains unchanged

### Requirement: Real Execution Path
The system SHALL support a real execution mode that evaluates workflows, persists execution state, and returns an execution identifier for result traceability. During real execution, if an active rule in `production` status fails evaluation or action execution, the system SHALL persist a transition of that rule status to `failed`; rules in `draft` SHALL remain `draft`, rules in `failed` SHALL remain `failed`, and rules in `disabled` SHALL remain excluded.

#### Scenario: Real execution
- **WHEN** execute is requested with dryRun=false
- **THEN** evaluation results are returned and execution state is persisted with a non-null execution identifier

#### Scenario: Real execution failure transitions production to failed
- **WHEN** execute is requested with dryRun=false and an active `production` rule fails
- **THEN** the rule status is persisted as `failed` and the response reports the transition

#### Scenario: Real execution failure keeps draft in draft
- **WHEN** execute is requested with dryRun=false and an active `draft` rule fails
- **THEN** the failure is reported and the rule status remains `draft`

#### Scenario: Real execution failure keeps failed in failed
- **WHEN** execute is requested with dryRun=false and an active `failed` rule fails
- **THEN** the failure is reported and the rule status remains `failed`
