# core-engine Specification

## Purpose
RulesEngine.Core domain behavior - validation and execution.

## Requirements

### Requirement: Workflow Schema Validation
The system SHALL validate workflow JSON against a versioned schema before execution.

#### Scenario: Valid workflow submission
- GIVEN a workflow JSON conforming to schema v1
- WHEN the workflow is submitted to the validation endpoint
- THEN the system returns a success response with no errors

#### Scenario: Invalid workflow submission
- GIVEN a workflow JSON with missing required nodes
- WHEN the workflow is submitted to the validation endpoint
- THEN the system returns a 400 Bad Request with structured error details

### Requirement: Workflow Execution
The system SHALL execute validated workflows using the RulesEngine library.

#### Scenario: Dry-run execution
- GIVEN a validated workflow and test input data
- WHEN the execute endpoint is called with dryRun=true
- THEN the system returns execution results without side effects

#### Scenario: Real execution
- GIVEN a validated workflow and production input data
- WHEN the execute endpoint is called with dryRun=false
- THEN the system persists execution state and returns results
