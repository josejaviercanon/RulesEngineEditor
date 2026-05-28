## MODIFIED Requirements

### Requirement: Visual Workflow Editing
The system SHALL provide a Blazor WebAssembly SPA with a visual workflow editor and MUST document the local LogicFlow runtime/documentation lookup paths used by `RulesEngine.UI`.

#### Scenario: Render editor canvas
- **GIVEN** a user authenticated in the Blazor SPA
- **WHEN** the user navigates to /editor
- **THEN** the LogicFlow.js canvas renders with a default node palette

#### Scenario: Interop data exchange
- **GIVEN** the user has drawn a workflow on the canvas
- **WHEN** the user clicks "Validate"
- **THEN** the SPA extracts the graph JSON via JS interop
- **AND** sends it to the Minimal API validation endpoint

#### Scenario: Document Blazor WASM LogicFlow distribution paths
- **WHEN** maintainers or agents update workflow editor guidance for `RulesEngine.UI`
- **THEN** documentation MUST identify `./src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/` as the compiled runtime asset location
- **AND** documentation MUST name `index.css` and `index.min.js` as the shipped UI bundle artifacts
- **AND** documentation MUST identify `/src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/docs` as the local markdown documentation root

#### Scenario: Enforce local-doc-first capability discovery
- **WHEN** an agent or contributor plans a LogicFlow feature for the workflow UI editor
- **THEN** they MUST check local LogicFlow docs first (`wwwroot` docs and local node docs) to find built-in, extension, or layout capabilities
- **AND** they MUST prefer official documented capabilities over custom reimplementation when a match exists
