## ADDED Requirements

### Requirement: RulesEngine Integration Contract
The system SHALL integrate with the RulesEngine library to register workflows, validate workflow models, and execute rule evaluations.

#### Scenario: RulesEngine registration and evaluation
- **GIVEN** a validated workflow definition
- **WHEN** the application invokes the execution service
- **THEN** the RulesEngine integration registers or updates workflow definitions and returns evaluation results

### Requirement: LogicFlow JS Interop Contract
The system SHALL expose a stable JS interop contract for LogicFlow.js canvas operations through a wrapper module.

#### Scenario: Retrieve graph data via interop
- **GIVEN** the editor canvas is initialized
- **WHEN** the UI requests current graph data
- **THEN** the interop wrapper returns graph JSON through getGraphData without leaking raw library internals to components

### Requirement: Radzen UI Component Contract
The system SHALL use Radzen components for editor forms, validation display, and workflow operation controls in a consistent UI contract.

#### Scenario: Validate action from Radzen UI
- **GIVEN** a user edits workflow content in form controls
- **WHEN** the user activates Validate from a Radzen action control
- **THEN** the UI dispatches a validation request and renders structured feedback in Radzen presentation components

### Requirement: EF Core Persistence Contract
The system SHALL use EF Core repositories and mappings to persist workflow definitions and metadata using a JSON definition strategy.

#### Scenario: Persist workflow definition
- **GIVEN** a create or update workflow command
- **WHEN** persistence is executed
- **THEN** EF Core stores the workflow definition payload according to configured JSON mapping rules

### Requirement: Minimal API Integration Contract
The system SHALL expose integration endpoints through Minimal API mappings for validation, execution, and workflow lifecycle operations.

#### Scenario: Integration endpoint handling
- **GIVEN** a workflow integration request
- **WHEN** a mapped Minimal API endpoint receives the request
- **THEN** the endpoint calls application services and returns HTTP responses with consistent status and payload contracts
