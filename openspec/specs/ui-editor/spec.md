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

#### Scenario: Enforce Blazorise UI composition
- GIVEN a contributor or agent implements or updates RulesEngine.UI pages
- WHEN selecting components for forms, data grids, dialogs, and action controls
- THEN they use Blazorise components and configured providers (`Blazorise`, `Blazorise.Tailwind`, `Blazorise.Icons.Lucide`)
- AND they do not introduce an alternate UI component library for app-level UI composition

### Requirement: Home page is the default workflow management entry point
The UI SHALL open to the Home page by default and SHALL present a workflow grid with one row per workflow identity showing active revision metadata and operator actions.

#### Scenario: Home route is the default page
- **WHEN** a user opens the RulesEngine.UI application
- **THEN** the Home page is shown as the initial route
- **AND** the workflow grid is loaded without requiring manual navigation

#### Scenario: Home workflow grid shows required columns
- **WHEN** the Home page workflow list is rendered
- **THEN** each row shows Actions, workflow Guid ID, latest workflow version, workflow name, and enablement state
- **AND** each row represents the active revision projection for a workflow identity

#### Scenario: Create workflow from Home actions
- **WHEN** the user clicks New in Home actions and saves a valid modal form
- **THEN** a new workflow is created without requiring system-only fields in user input
- **AND** the grid refreshes to show the new workflow row

#### Scenario: Edit workflow from Home actions with version controls
- **WHEN** the user opens Edit for a workflow row
- **THEN** a modal shows editable workflow data and a version list for that workflow identity
- **AND** the user can activate a selected version and set enablement according to lifecycle constraints

### Requirement: Rules page supports expandable workflow-to-rules management
The UI SHALL provide a Rules page that reuses workflow grid context, includes row expansion for workflow rules, and supports rule create/edit operations through modal dialogs.

#### Scenario: Rules page workflow row expansion shows nested rules grid
- **WHEN** the user opens Rules and expands a workflow row
- **THEN** a detail grid is shown for that workflow with columns Actions, Guid ID, Name, and Version

#### Scenario: Create rule for selected workflow
- **WHEN** the user clicks create-rule action for a workflow and saves valid rule data
- **THEN** a new rule revision is persisted for the selected workflow context
- **AND** the nested rules grid refreshes

#### Scenario: Edit rule validates expression and preserves modal state on failure
- **WHEN** the user edits rule Name/Expression and submits invalid expression content
- **THEN** the UI shows validation information returned by the system
- **AND** the modal remains open with user-entered values for correction
