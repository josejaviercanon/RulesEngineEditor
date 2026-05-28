## MODIFIED Requirements

### Requirement: LogicFlow JS Interop Contract
The system SHALL expose a stable JS interop contract for LogicFlow.js canvas operations through a wrapper module and MUST define agentic integration guidance for LogicFlow package roles and documentation lookup order.

#### Scenario: Retrieve graph data via interop
- **GIVEN** the editor canvas is initialized
- **WHEN** the UI requests current graph data
- **THEN** the interop wrapper returns graph JSON through getGraphData without leaking raw library internals to components

#### Scenario: Define package roles for integration planning
- **WHEN** workflow editor integration docs are updated
- **THEN** docs MUST describe package roles as:
- **AND** `@logicflow/core` provides the core graph editor runtime (canvas, nodes, edges, models, events, rendering, themes, and basic interactions)
- **AND** `@logicflow/extension` provides official plugins for common product features
- **AND** `@logicflow/layout` provides official layout plugins for automatic graph layout

#### Scenario: Define node-module docs lookup locations
- **WHEN** integration guidance references upstream LogicFlow docs
- **THEN** docs MUST include `node_modules/@logicflow/core/dist/docs/` as the local node documentation root
- **AND** docs MUST include `node_modules/@logicflow/core/dist/docs/tutorial/extension/` for `@logicflow/extension` and `@logicflow/layout` capabilities

#### Scenario: Guard package installation in agentic workflows
- **WHEN** an official LogicFlow package is needed but not installed
- **THEN** the contributor or agent MUST ask the user before installing the package

#### Scenario: Preserve standard agent-rules snippet markers
- **WHEN** repository documentation includes LogicFlow agent guidance
- **THEN** guidance MUST preserve the standard marker format using `<!-- BEGIN:logicflow-agent-rules -->` and `<!-- END:logicflow-agent-rules -->`
- **AND** the content between markers MUST include local docs path, package roles, extension/layout docs path, local-doc-first policy, and ask-before-install policy
