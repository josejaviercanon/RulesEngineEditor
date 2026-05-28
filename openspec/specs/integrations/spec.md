# integrations Specification

## Purpose
Define integration contracts with RulesEngine, LogicFlow interop, Radzen UI, EF Core persistence, PostgreSQL rules storage, and Minimal API integration endpoints.

## Requirements

### Requirement: RulesEngine Integration Contract
The system SHALL integrate with the RulesEngine library to register workflows, validate workflow models, and execute rule evaluations.

#### Scenario: RulesEngine registration and evaluation
- GIVEN a validated workflow definition
- WHEN the application invokes the execution service
- THEN the RulesEngine integration registers or updates workflow definitions and returns evaluation results

### Requirement: LogicFlow JS Interop Contract
The system SHALL expose a stable JS interop contract for LogicFlow.js canvas operations through a wrapper module and MUST define agentic integration guidance for LogicFlow package roles and documentation lookup order.

#### Scenario: Retrieve graph data via interop
- GIVEN the editor canvas is initialized
- WHEN the UI requests current graph data
- THEN the interop wrapper returns graph JSON through getGraphData without leaking raw library internals to components

#### Scenario: Define package roles for integration planning
- GIVEN workflow editor integration docs are updated
- WHEN package responsibilities are documented
- THEN docs describe `@logicflow/core` as the core graph editor runtime (canvas, nodes, edges, models, events, rendering, themes, and basic interactions)
- AND docs describe `@logicflow/extension` as official plugins for common product features
- AND docs describe `@logicflow/layout` as official layout plugins for automatic graph layout

#### Scenario: Define node-module docs lookup locations
- GIVEN integration guidance references upstream LogicFlow docs
- WHEN local documentation locations are listed
- THEN docs include node_modules/@logicflow/core/dist/docs/ as the local node documentation root
- AND docs include node_modules/@logicflow/core/dist/docs/tutorial/extension/ for `@logicflow/extension` and `@logicflow/layout` capabilities

#### Scenario: Guard package installation in agentic workflows
- GIVEN an official LogicFlow package is needed but not installed
- WHEN the contributor or agent is preparing to install it
- THEN they ask the user before installation

#### Scenario: Preserve standard agent-rules snippet markers
- GIVEN repository documentation includes LogicFlow agent guidance
- WHEN guidance snippets are authored or updated
- THEN guidance preserves marker format using <!-- BEGIN:logicflow-agent-rules --> and <!-- END:logicflow-agent-rules -->
- AND content between markers includes local docs path, package roles, extension/layout docs path, local-doc-first policy, and ask-before-install policy

### Requirement: Radzen UI Component Contract
The system SHALL use Radzen components for editor forms, validation display, and workflow operation controls in a consistent UI contract.

#### Scenario: Validate action from Radzen UI
- GIVEN a user edits workflow content in form controls
- WHEN the user activates Validate from a Radzen action control
- THEN the UI dispatches a validation request and renders structured feedback in Radzen presentation components

### Requirement: EF Core Persistence Contract
The system SHALL use EF Core repositories and mappings to persist workflow definitions and metadata using a JSON definition strategy.

#### Scenario: Persist workflow definition
- GIVEN a create or update workflow command
- WHEN persistence is executed
- THEN EF Core stores the workflow definition payload according to configured JSON mapping rules

### Requirement: PostgreSQL Rules Table Integration Contract
The system SHALL integrate EF Core persistence with PostgreSQL 18 using a rules table contract that includes Id, Name, Expression, RuleJson, Version, IsActive, EffectiveFromUtc, and EffectiveToUtc.

#### Scenario: Persist rule record into canonical rules table
- GIVEN the persistence layer is configured for PostgreSQL 18
- WHEN a rule definition is saved
- THEN the record is written to rules with primary key PK_rules on Id and the contract-compatible field set

### Requirement: Minimal API Integration Contract
The system SHALL expose integration endpoints through Minimal API mappings for validation, execution, and workflow lifecycle operations.

#### Scenario: Integration endpoint handling
- GIVEN a workflow integration request
- WHEN a mapped Minimal API endpoint receives the request
- THEN the endpoint calls application services and returns HTTP responses with consistent status and payload contracts
