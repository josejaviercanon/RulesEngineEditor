## Why

Workflow and rule editing currently create new versions on every save, which makes normal maintenance noisy and error-prone. The UI also lacks key operational controls (delete and validate actions) and clear version visibility, so users cannot safely distinguish update-in-place from intentional version creation.

## What Changes

- Home page workflow grid adds an `Active Version` column immediately after `Guid ID` to show the currently active workflow version separately from version count (`Last Version`).
- Home page edit workflow modal changes `Save` behavior to update only the selected version, not create a new version.
- Home page edit workflow modal adds an explicit `Create New Version` action with yes/no confirmation.
- Home page new workflow modal sets `Enable` toggle to OFF by default for newly created workflows.
- Backend workflow-version creation logic always adds exactly one `Default Rule` for each new workflow version and does not create any additional rule by default.
- Rules page edit rule modal changes `Save` behavior to update only the selected rule version, not create a new version.
- Rules page edit rule modal adds an explicit `Create New Version` action with yes/no confirmation.
- Rules grid action column adds `Delete` with yes/no confirmation for rule deletion.
- Rules grid action column adds `Validate` to validate a single rule expression and display success/errors.
- Workflows main grid action column adds `Validate` to validate the full workflow and display success/errors.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `ui-editor`: update modal and grid behaviors for versioning, delete, and validate actions.
- `rule-versioning-and-activation`: separate save-in-place from explicit rule version creation.
- `workflow-version-history`: separate save-in-place from explicit workflow version creation and expose active version in list views.
- `workflow-enablement-controls`: default newly created workflows to disabled (`Enable` OFF).
- `workflow-rule-collections`: enforce workflow version rule initialization semantics and rule deletion behavior.
- `workflow-compile-validate`: add user-invoked validation for workflow-level and rule-expression-level checks.

## Impact

- Affected projects: `src/RulesEngine.UI`, `src/RulesEngine.API`, and related application/core services handling workflow/rule versioning.
- API behavior changes for edit endpoints and version-creation endpoints to preserve selected version on save.
- UI action model expands with explicit version creation and validation commands plus confirmation dialogs.
- Validation feedback surfaces in UI for both rule expressions and full workflows.
