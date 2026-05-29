## Why

The UI does not currently provide the requested menu-driven workflow management experience for Home and Rules with modal create/edit flows. We need a consistent Blazorise-based UX to manage workflows and rules, including version activation and enablement constraints that align with backend invariants.

## What Changes

- Add a main-menu Home page as the default landing page that lists one row per workflow identity (active revision projection) with columns: Actions, Guid ID, last workflow version, workflow name, and enablement state.
- Add Home actions for creating and editing workflows using modal dialogs with type-appropriate form inputs, save/cancel actions, and automatic grid refresh after successful save.
- Extend Home edit flow to include workflow version selection and active-version management, including enforcement that only one workflow revision is active per workflow identity and enablement behavior remains valid.
- Add a Rules page that uses the workflow grid with a details expander per workflow row and a nested rules grid.
- Add Rules actions for creating rules for a selected workflow and editing existing rule name/expression in a modal dialog.
- Add rule expression validation on save with user-visible validation feedback when expression validation fails.
- Ensure UI actions use existing API contracts for workflow/rule version activation and enablement semantics.

## Capabilities

### New Capabilities
- (none)

### Modified Capabilities
- `ui-editor`: Add menu-driven Home/Rules pages, data grids, row-detail expansion, and modal create/edit experiences for workflows and rules.
- `api-surface`: Confirm/extend endpoints and validation responses needed by UI workflow/rule create-edit, version listing/activation, and expression validation feedback.
- `workflow-lifecycle`: Align workflow active-version switching and enablement behavior exposed in UI with lifecycle invariants.
- `rule-versioning-and-activation`: Align rules detail grid editing/version behavior and activation semantics used from UI interactions.

## Impact

- Affected UI: `src/RulesEngine.UI` navigation, pages, Blazorise grids/modals/forms, state handling, and API client integration.
- Affected application/API: request/response flows used by UI for workflow and rule create/update/version operations and validation messaging.
- Affected validation behavior: rule expression validation feedback surfaced to users during save.
- Affected tests: UI/component tests for page behavior and API integration tests for workflow/rule version and validation paths used by the UI.
