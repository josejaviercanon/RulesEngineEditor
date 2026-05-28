# ui-editor Specification

## Purpose
Define Blazor WebAssembly editor behavior, user interaction flows, and JS interop contracts for visual workflow authoring.

## Requirements

### Requirement: Visual Workflow Editing
The system SHALL provide a Blazor WebAssembly SPA with a visual workflow editor and MUST document the local LogicFlow runtime/documentation lookup paths used by `RulesEngine.UI`.

#### Scenario: Render editor canvas
- GIVEN a user authenticated in the Blazor SPA
- WHEN the user navigates to /editor
- THEN the LogicFlow.js canvas renders with a default node palette

#### Scenario: Interop data exchange
- GIVEN the user has drawn a workflow on the canvas
- WHEN the user clicks "Validate"
- THEN the SPA extracts the graph JSON via JS interop
- AND sends it to the Minimal API validation endpoint

#### Scenario: Document Blazor WASM LogicFlow distribution paths
- GIVEN maintainers or agents update workflow editor guidance for RulesEngine.UI
- WHEN documentation is updated
- THEN it identifies ./src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/ as the compiled runtime asset location
- AND it names index.css and index.min.js as the shipped UI bundle artifacts
- AND it identifies /src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/docs as the local markdown documentation root

#### Scenario: Enforce local-doc-first capability discovery
- GIVEN an agent or contributor plans a LogicFlow feature for the workflow UI editor
- WHEN they evaluate available capabilities
- THEN they check local LogicFlow docs first (wwwroot docs and local node docs) to find built-in, extension, or layout capabilities
- AND they prefer official documented capabilities over custom reimplementation when a match exists
