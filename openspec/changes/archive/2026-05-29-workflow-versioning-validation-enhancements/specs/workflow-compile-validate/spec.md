## ADDED Requirements

### Requirement: Rule expression validation endpoint returns user-consumable results
The system SHALL expose a rule-expression validation operation that evaluates a selected rule expression using RulesEngine-compatible validation and returns structured validity results without persistence side effects.

#### Scenario: Validate expression returns success for valid rule
- **WHEN** the client validates a rule expression that compiles and passes semantic checks
- **THEN** the response indicates `IsValid=true`
- **AND** no workflow or rule data is persisted or modified

#### Scenario: Validate expression returns errors for invalid rule
- **WHEN** the client validates a rule expression that fails syntax or semantic checks
- **THEN** the response indicates `IsValid=false`
- **AND** the response includes one or more validation error messages

### Requirement: Workflow validation is invocable from workflow action context
The system SHALL support explicit workflow-level validation for selected workflow rows and SHALL return structured success/error results for UI display.

#### Scenario: Validate selected workflow returns success
- **WHEN** the client validates a selected workflow with valid rules and expressions
- **THEN** the response indicates `IsValid=true`

#### Scenario: Validate selected workflow returns errors
- **WHEN** the client validates a selected workflow that contains invalid rule expressions or structural issues
- **THEN** the response indicates `IsValid=false`
- **AND** the response includes workflow/rule-level validation errors for user display
