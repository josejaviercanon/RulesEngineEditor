## Why

The current Rules Page supports tabular workflow/rule editing, but users cannot visualize rule execution flow, which makes complex workflows harder to understand and maintain. Adding a first-class visual editor now improves usability, supports safer workflow authoring, and aligns the UI with the existing LogicFlow integration direction.

## What Changes

- Add a new `Visual Editor` page in `RulesEngine.UI` that hosts a LogicFlow.js-based workflow diagram with a blank default canvas.
- Add top-level `Load` and `Save` actions in the Visual Editor:
  - `Load` opens a modal listing active workflow versions and loads the selected workflow.
  - `Save` persists workflow and rule property changes, regenerates `WorkflowJson` and each rule `RuleJson`, and performs save-time validation flow.
- Add a `Visual Editor` command action in the Rules data grid that navigates to the visual editor and auto-loads the selected workflow.
- Add rule-node authoring and linking in the diagram, including rule property editing through a modal equivalent to the existing Rules Page rule editor.
- Introduce `ExecuteOrder` numeric field for rules in model/database/API/UI and use it to drive execution ordering in both the Rules detail grid and visual link representation.
- Extend rule editor modals (Rules Page and Visual Editor) to include `ExecuteOrder`.
- Implement save validation with dry-run behavior and user decision path:
  - If validation errors exist, allow user to save as draft with warnings or cancel and continue editing.
  - Even draft saves must enforce syntactically valid JSON persistence to prevent future load failures.

## Capabilities

### New Capabilities
- `visual-workflow-editor`: LogicFlow-based visual workflow designer experience with load/save and node-link editing flows.

### Modified Capabilities
- `ui-editor`: Add Visual Editor navigation/entry points, workflow load interactions, and parity rule-property editing behavior.
- `workflow-rule-collections`: Add and enforce `ExecuteOrder` semantics for rule ordering and node connection order.
- `workflow-json-persistence`: Expand save behavior to regenerate persisted workflow/rule JSON from visual edits with draft-save safeguards.
- `workflow-compile-validate`: Add pre-save validation decision flow (warn/save-draft/cancel) while preserving JSON correctness guarantees.

## Impact

- UI: `src/RulesEngine.UI` pages, components, JS interop for LogicFlow, routing/navigation, modal workflows.
- API/Application/Core: workflow/rule load/save contracts and dry-run validation pathways.
- Infrastructure/Data: rule persistence model and migration updates for `ExecuteOrder`.
- Shared contracts: DTOs/view models carrying `ExecuteOrder`, workflow load/save payloads, and JSON regeneration outcomes.
- Tests: UI behavior tests, workflow/rule ordering tests, save validation decision-path tests, and JSON integrity tests.
