## MODIFIED Requirements

### Requirement: Workflow Schema Validation
The system SHALL validate workflow JSON against a versioned schema before execution and SHALL provide resolved schema version information with validation outcomes.

#### Scenario: Valid workflow submission
- **WHEN** a workflow JSON conforming to a supported schema version is submitted to validation
- **THEN** the system returns a success result with resolved schema version and no errors

#### Scenario: Invalid workflow submission
- **WHEN** a workflow JSON with missing or invalid required nodes is submitted to validation
- **THEN** the system returns a structured validation failure payload containing resolved schema version and error details

### Requirement: Workflow Execution
The system SHALL execute validated workflows using the RulesEngine library through backend execution services and SHALL map engine results to a stable backend execution result contract.

#### Scenario: Dry-run execution
- **WHEN** execution is requested in dry-run mode for a validated workflow
- **THEN** the system returns execution results without persisting execution state

#### Scenario: Real execution
- **WHEN** execution is requested in persisted mode for a validated workflow
- **THEN** the system persists execution state and returns execution results with persisted execution identity
