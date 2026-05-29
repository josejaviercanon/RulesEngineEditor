## Context

The Rules page currently shows workflow/rule version metadata incompletely for operational version control. `Last Version` is present, but operators cannot directly see the currently active revision in all relevant grids. In addition, the rule edit modal does not yet offer workflow-parity version navigation/activation behavior.

This change spans UI (`RulesEngine.UI`), API contracts (`RulesEngine.API`), and persistence/query logic (`RulesEngine.Infrastructure` and application layer mapping) because active-version metadata and active-only projections must remain consistent across views and save operations.

Constraints:
- Preserve existing workflow versioning semantics and rule expression validation.
- Maintain invariant: exactly one active version per `RuleGuidId`.
- Keep existing column order requirements in Rules page grids.

## Goals / Non-Goals

**Goals:**
- Expose `Active Version` separately from `Last Version` in Rules page workflow and nested rule grids.
- Return only active rule revisions in the workflow-details rules grid on Rules page.
- Add rule-edit modal version list + active toggle, and reload full rule data when selected version changes.
- Ensure save flow that changes active version performs deactivate-old then activate-new semantics safely.

**Non-Goals:**
- No redesign of general workflow/rule CRUD UX outside requested grids and edit modal.
- No change to explicit create-new-version intent semantics.
- No broad schema redesign beyond fields/queries needed for active-version projection and activation updates.

## Decisions

1. Keep projections explicit with separate `ActiveVersion` and `LastVersion` fields.
Rationale: avoids overloading meaning and aligns with existing workflow version-history semantics.
Alternative considered: infer active from max version and flags client-side. Rejected because active version may not be latest.

2. Use active-only rule retrieval for nested rules grid at API/query level.
Rationale: enforcing one-row-per-rule-identity server-side prevents duplicate logical rules and simplifies UI state.
Alternative considered: fetch all versions and filter in UI. Rejected due to inconsistent behavior risk and over-fetching.

3. Reuse workflow modal versioning interaction pattern for rule edit modal.
Rationale: operator familiarity and reduced behavioral divergence.
Alternative considered: separate lightweight activation dialog. Rejected because requirement includes full version data reload and edit parity.

4. Enforce activation switch inside one backend operation boundary.
Rationale: single-active invariant must hold under concurrent requests; operation should atomically deactivate previous active revision and activate selected revision for the same `RuleGuidId`.
Alternative considered: two client-driven calls. Rejected due to race risk and partial-update hazards.

5. On modal version selection change, reload the selected persisted version payload (expression + all editable properties + active state).
Rationale: selected-version editing must be explicit and deterministic.
Alternative considered: patch only expression/active fields. Rejected because requirement demands all properties reload.

## Risks / Trade-offs

- [Risk] Concurrency conflicts during activation switching could leave ambiguous state. -> Mitigation: backend transactional update for same `RuleGuidId` with deterministic active target and post-condition check.
- [Risk] UI state desynchronization when rapidly switching versions in modal. -> Mitigation: disable save while version payload is loading and bind form strictly to selected version model.
- [Risk] Existing tests may assume rules grid includes non-active revisions. -> Mitigation: update tests and fixtures to assert active-only behavior.
- [Trade-off] Additional API/query fields increase DTO complexity slightly. -> Benefit: clear operator semantics and fewer UI-derived assumptions.

## Migration Plan

1. Extend backend list/query projections for workflow and rule grid rows to include active-version metadata where missing.
2. Update nested rules endpoint/query mode on Rules page data path to active-only per `RuleGuidId`.
3. Add/adjust backend activation update command to guarantee deactivate-old then activate-new in one operation.
4. Update Rules page main grid and nested rules grid column definitions/order.
5. Update rule edit modal UI with versions list, active toggle, and selected-version reload flow.
6. Add/adjust automated tests for:
   - Column presence/order and active-only rows in Rules page data contracts.
   - Rule activation invariant (only one active version).
   - Modal version-switch reload behavior.
7. Rollback strategy: revert UI column/modal changes and restore previous query mode while preserving persisted historical data.

## Open Questions

- Should rule versions list in the edit modal be sorted descending (latest first) or ascending for parity with workflow modal behavior?
- Should activation changes made from the rule edit modal require a dedicated confirmation prompt, or follow existing save-confirmation conventions?