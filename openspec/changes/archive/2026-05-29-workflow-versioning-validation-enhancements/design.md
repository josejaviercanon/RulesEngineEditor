## Context

The current workflow and rule edit flows in `RulesEngine.UI` implicitly create new versions whenever users click save. This conflates two distinct intents: updating the current selected version and creating a new version lineage entry. It also introduces accidental version growth and unclear operator expectations.

In parallel, workflow/rule grids lack operational actions needed by users to manage and verify configurations in-place (delete and validate). The backend (`RulesEngine.API`) currently owns versioning and rule initialization behavior, so the design must preserve consistency between UI intent and API behavior.

Constraints:
- Existing workflow/rule version history and activation semantics must remain backward compatible for existing data.
- Validation actions should reuse RulesEngine validator behavior already used by compile/validation paths.
- UI changes should minimize disruption to current modal/grid layouts.

Stakeholders:
- Workflow designers/operators using Home and Rules pages.
- Backend maintainers responsible for version and rule lifecycle semantics.

## Goals / Non-Goals

**Goals:**
- Separate `Save` (update selected version) from `Create New Version` (intentional version increment) for workflows and rules.
- Add explicit confirmation dialogs before creating a new version or deleting a rule.
- Add `Active Version` visibility in the workflow home grid.
- Default newly created workflows to disabled (`Enable = false`).
- Enforce deterministic workflow-version initialization: exactly one `Default Rule`, no additional automatic rules.
- Add validation actions for single rule expression and full workflow with clear result reporting.

**Non-Goals:**
- Redesigning the overall page layout/navigation.
- Replacing the underlying RulesEngine validation library.
- Introducing bulk delete, bulk validate, or background validation jobs.
- Changing historical version numbering rules beyond explicit creation behavior.

## Decisions

1. Save-in-place vs version creation is modeled as separate API intents
- Decision: Keep `Save` operations bound to the currently selected version identifier and update only that version's payload.
- Decision: Introduce explicit create-version commands/endpoints (or command flags) that are called only from `Create New Version` buttons.
- Rationale: Prevent accidental version proliferation and align UI actions with user intent.
- Alternative considered: Keep current single save endpoint with optional query flag. Rejected because it is easier to misuse and harder to reason about in UI state transitions.

2. UI version actions use confirmation dialogs for destructive or lineage-changing operations
- Decision: Add yes/no confirmation dialog before creating a new workflow/rule version and before deleting a rule.
- Rationale: Adds safety boundary for irreversible or high-impact actions.
- Alternative considered: No confirmation and rely on undo/versioning. Rejected due to unclear rollback UX and user risk.

3. Workflow grid includes explicit active-version projection
- Decision: Extend workflow list DTO/view model to include active version index/id and render `Active Version` after `Guid ID`.
- Rationale: Operators need quick distinction between active version and total version count (`Last Version`).
- Alternative considered: Tooltip or details-only display. Rejected because active status must be visible in primary grid scan.

4. New workflow defaults to disabled
- Decision: Initialize create-workflow form state with `Enable = false` and ensure backend create command respects explicit false when omitted.
- Rationale: Safe-by-default behavior for newly authored workflows.
- Alternative considered: Keep current default and require manual disable. Rejected because it can unintentionally expose incomplete workflows.

5. New workflow version initialization creates only one default rule
- Decision: Version creation path always creates exactly one `Default Rule` entry and no other implicit rule records.
- Rationale: Deterministic seed behavior and minimal boilerplate while avoiding hidden rule generation.
- Alternative considered: No default rule. Rejected to preserve baseline executable structure expected by existing flows.

6. Validation actions use dedicated read-only endpoints/services
- Decision: Add/extend API operations for:
  - Rule expression validation (single rule)
  - Workflow validation (full workflow)
- Decision: Return structured validation results (isValid, messages/errors) and show them in modal/notification UI.
- Rationale: Keeps validation side-effect free and reusable across UI actions and tests.
- Alternative considered: Client-side-only validation. Rejected due to potential drift from backend RulesEngine behavior.

## Risks / Trade-offs

- [Risk] Save/update semantics may accidentally still trigger version creation in shared backend code paths. -> Mitigation: split command handlers or add explicit branching with unit tests for update vs create-version paths.
- [Risk] DTO/API contract changes for active version may break older UI assumptions. -> Mitigation: version DTO fields additively and keep existing fields unchanged.
- [Risk] Confirmation dialog fatigue for frequent users. -> Mitigation: restrict confirmations to create-version and delete only.
- [Risk] Validation error payloads may be too technical for end users. -> Mitigation: map backend messages to concise UI text while preserving raw details in expandable section.
- [Risk] Rule deletion may affect workflow integrity unexpectedly. -> Mitigation: validate remaining workflow/rule set after delete and surface actionable errors.

## Migration Plan

1. Introduce additive API contract updates (active version projection and validation result DTOs).
2. Implement backend separation for update vs create-version logic for workflows and rules.
3. Update UI modal actions/buttons and dialog flows.
4. Add/delete/validate actions in rules/workflow grids.
5. Backfill tests for versioning behavior, default enable state, default-rule creation, delete, and validation flows.
6. Deploy with backward-compatible DTO additions; rollback by disabling new UI actions and reverting to prior command handlers if required.

## Open Questions

- Should delete rule be hard delete or soft delete with audit metadata in this iteration?
- Should validation results render in modal dialogs, toast summaries, or both for long error lists?
- For workflow-level validate, should validation run against current persisted version only or support staged unsaved edits in future iterations?
