## Why

Rules operators cannot currently distinguish between each workflow or rule identity's active version and total retained versions in all Rules page views, which causes ambiguity during audits and operational edits. The rule edit modal also lacks workflow-parity version controls, making it hard to activate the intended rule revision safely.

## What Changes

- Add an `Active Version` column to the Rules page workflow main grid immediately after `Guid ID`, while keeping `Last Version` as total retained version count.
- Add an `Active Version` column to the Rules page workflow-details rules grid immediately before `Last Version`.
- Change the workflow-details rules grid query/projection to show only active rule revisions (`IsActive = true`) for the selected workflow, with one row per `RuleGuidId`.
- Extend the edit-rule modal with version management parity to workflow editing:
  - Add a versions list selector for all retained revisions of the selected rule identity.
  - Add an active-version toggle for the selected rule version.
  - Reload all modal fields (including expression and active toggle state) when the selected version changes.
- Enforce single-active-version invariant when saving rule activation changes by deactivating the old active version before activating the new selected version.
- Keep expression validation and existing save semantics intact for edit-in-place vs explicit version creation flows.

## Capabilities

### New Capabilities
- None.

### Modified Capabilities
- `ui-editor`: Rules page grids and edit-rule modal behavior change to show active version metadata, active-only detail rows, and selectable rule-version activation controls.
- `rule-versioning-and-activation`: Activation/save flows must guarantee exactly one active rule revision per `RuleGuidId` during updates initiated from rule editing.
- `workflow-version-history`: Rules page projections require distinct active-version metadata alongside latest-version counts in workflow/rule grids.
- `api-surface`: Rule version list, active-only retrieval, and activation update contracts are required to support modal version selection and active-toggle persistence.

## Impact

- Affected UI code in `src/RulesEngine.UI` for Rules page grid definitions, data loading, and edit-rule modal state management.
- Affected API/application/infrastructure code in `src/RulesEngine.API` and supporting layers for active-only rule retrieval and activation update semantics.
- Potential data-access updates in repositories/queries to return active-version metadata and enforce activation ordering safely.
- Tests should be updated for API invariants, repository behavior, and UI-facing contract expectations around active-version columns and modal version switching.