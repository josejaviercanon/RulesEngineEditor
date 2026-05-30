# visual-workflow-editor Specification

## Purpose
Define dedicated visual workflow editor behaviors and save/load lifecycle for LogicFlow-based workflow authoring.

## Requirements
### Requirement: Visual Editor initializes with blank LogicFlow canvas
The system SHALL provide a `Visual Editor` menu entry and route that loads a blank LogicFlow workflow canvas by default when no workflow is selected.

#### Scenario: Open Visual Editor from main navigation
- **WHEN** the user clicks `Visual Editor` from the application navigation
- **THEN** the UI opens the visual editor page
- **AND** a blank diagram canvas is initialized with no persisted workflow loaded

### Requirement: Visual Editor provides workflow load flow for active versions
The visual editor SHALL provide a `Load` action that opens a modal listing workflow active versions and MUST load the selected workflow when the user accepts.

#### Scenario: Load modal lists active workflow versions
- **WHEN** the user clicks `Load` in the visual editor toolbar
- **THEN** a modal is shown with a list box of workflows limited to active versions
- **AND** the modal includes `Accept` and `Cancel` actions

#### Scenario: Accept loads selected workflow
- **WHEN** the user selects a workflow active version and clicks `Accept`
- **THEN** the selected workflow is loaded into the diagram and editor state
- **AND** the modal closes

#### Scenario: Cancel closes load modal without changes
- **WHEN** the user clicks `Cancel` in the load modal
- **THEN** the modal closes
- **AND** the currently loaded visual editor state remains unchanged

### Requirement: Visual Editor supports node create, link, and rule property editing
The visual editor SHALL allow users to create rule nodes, connect nodes to represent execution links, and edit rule properties using the same property structure used on the Rules Page rule editor.

#### Scenario: Create and connect rule nodes
- **WHEN** the user creates two rule nodes and links them
- **THEN** the editor persists node and edge state in the in-memory workflow model
- **AND** the execution linkage is available for save-time ordering normalization

#### Scenario: Edit rule properties from a node
- **WHEN** the user opens node properties for a rule node
- **THEN** the rule editor modal shows the same fields and options/types used in the Rules Page
- **AND** changes are reflected in the visual editor state when saved

### Requirement: Visual Editor save regenerates canonical JSON artifacts
The visual editor `Save` operation SHALL persist workflow and rule property updates and MUST regenerate canonical `WorkflowJson` for the workflow and `RuleJson` for each rule revision.

#### Scenario: Save persists properties and regenerated JSON
- **WHEN** the user clicks `Save` with a modified visual workflow
- **THEN** the backend saves workflow/rule structured fields
- **AND** the backend regenerates and persists `WorkflowJson` and each `RuleJson` from the current editor model

### Requirement: Visual Editor save enforces validate-before-save with draft option
The visual editor SHALL run validation before save and MUST present a warning decision when validation errors exist, allowing `Save as Draft` or `Cancel`, while always enforcing syntactically valid JSON persistence.

#### Scenario: Validation passes and save succeeds
- **WHEN** the user clicks `Save` and validation returns no errors
- **THEN** the workflow is saved without warning dialog

#### Scenario: Validation errors allow save as draft
- **WHEN** the user clicks `Save` and validation returns errors
- **THEN** a warning is shown with options to `Save as Draft` or `Cancel`
- **AND** choosing `Save as Draft` persists the workflow only if generated JSON is syntactically valid

#### Scenario: Cancel keeps editor in edit mode
- **WHEN** the validation warning is shown and the user clicks `Cancel`
- **THEN** no persistence occurs
- **AND** the user remains in the visual editor with current unsaved changes
