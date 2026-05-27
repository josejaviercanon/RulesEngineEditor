# ui-editor Specification

## Purpose
Define Blazor WebAssembly editor behavior, user interaction flows, and JS interop contracts for visual workflow authoring.

## Requirements

### Requirement: Visual Workflow Editing
The system SHALL provide a Blazor WebAssembly SPA with a visual workflow editor.

#### Scenario: Render editor canvas
- GIVEN a user authenticated in the Blazor SPA
- WHEN the user navigates to /editor
- THEN the LogicFlow.js canvas renders with a default node palette

#### Scenario: Interop data exchange
- GIVEN the user has drawn a workflow on the canvas
- WHEN the user clicks "Validate"
- THEN the SPA extracts the graph JSON via JS interop
- AND sends it to the Minimal API validation endpoint
