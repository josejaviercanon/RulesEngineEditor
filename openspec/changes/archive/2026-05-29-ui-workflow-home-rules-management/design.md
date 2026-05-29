## Context

The backend already supports workflow and rule versioning semantics, including activation and workflow-level enable/disable operations, while the UI does not yet expose a structured menu-based management experience. The requested change introduces two operational pages in `RulesEngine.UI` (Home and Rules) that share workflow listing context but differ by actions and nested rule operations.

Constraints:
- UI must use Blazorise components already configured in `RulesEngine.UI` (Tailwind + Lucide icons).
- Home must display one row per workflow identity, represented by the currently active revision and its latest metadata needed by operators.
- Workflow activation/enablement must preserve backend invariant behavior: single active revision per workflow identity; enablement transitions remain valid with active state.
- Rules page must support expanding a workflow row into a nested rules grid and provide rule create/edit with expression validation feedback.
- Rule expression validation needs actionable user feedback on failure without losing user input.

Stakeholders:
- Workflow operators who need low-friction create/edit/version actions.
- Product/development teams that need safe lifecycle transitions surfaced in UI.
- API maintainers who need UI behavior aligned with existing command/query contracts.

## Goals / Non-Goals

**Goals:**
- Provide Home as the default page with workflow management data grid and modal create/edit flows.
- Provide Rules page with workflow grid, expandable nested rules grid, and rule create/edit flows.
- Surface workflow version selection/activation and enablement controls in edit workflow flow while preserving lifecycle invariants.
- Validate rule expressions on save and render user-readable validation feedback when invalid.
- Reuse shared UI state/services to keep Home and Rules behavior consistent.

**Non-Goals:**
- Redesign backend persistence or rule execution engine semantics.
- Introduce a new UI framework or replace Blazorise components.
- Implement unrelated workflow analytics, bulk operations, or permission model changes.
- Add new long-running background orchestration for UI actions.

## Decisions

1. Use two routable pages with shared workflow query/view-model service.
- Rationale: Home and Rules differ in interaction patterns, but both depend on identical workflow identity projection and refresh semantics.
- Alternative considered: one page with tabbed mode switching. Rejected because the interaction complexity (nested grids + separate modals) increases state coupling and makes testing harder.

2. Represent "one row per workflow" by active revision projection from API list endpoint.
- Rationale: this aligns with existing lifecycle specs and avoids client-side dedup/version conflict logic.
- Alternative considered: fetch all workflow versions and aggregate client-side. Rejected due to unnecessary payload/logic complexity and potential divergence from server invariants.

3. Implement create/edit flows with dedicated modal state objects and validation summaries.
- Rationale: explicit modal state avoids accidental cross-page state leaks and allows deterministic reset/save/cancel behavior.
- Alternative considered: inline editing in grid rows. Rejected because version activation and rule expression editing require multi-field contextual validation better suited to dialogs.

4. Expose workflow version operations through explicit version list selection in edit modal.
- Rationale: user requirement calls for selecting versions and toggling active/enabled state; explicit controls improve operator intent and auditability.
- Alternative considered: implicit activation on save of metadata edits. Rejected because it can trigger accidental activation changes when only editing name/flags.

5. Add a nested details data grid on Rules page using row-expansion pattern.
- Rationale: keeps workflow context visible while enabling per-workflow rule management.
- Alternative considered: navigate to a separate workflow-rules page. Rejected because it increases navigation friction and loses side-by-side workflow context.

6. Perform rule expression validation via backend validation endpoint/command before final save commit when supported, otherwise via save response handling.
- Rationale: preserves single source of truth for expression validity and supports parity with execution/runtime parser constraints.
- Alternative considered: client-only expression parsing. Rejected because backend parser behavior is authoritative and may differ from a client approximation.

## Risks / Trade-offs

- [Backend endpoint shape may not exactly match UI flow assumptions] -> Mitigation: map UI actions to existing API contracts first and add targeted API deltas only where required by spec changes.
- [Modal forms for complex workflow/rule data can become hard to maintain] -> Mitigation: factor reusable form fragments and dedicated state models per modal.
- [Nested data grid expansion can increase rendering cost with many rows] -> Mitigation: lazy-load rules for expanded rows only and cache per workflow row during session.
- [User confusion around active vs enabled semantics] -> Mitigation: add clear labels/help text and enforce server-returned validation messages directly in modal alerts.
- [Validation failures could discard pending form values] -> Mitigation: keep modal state unchanged on failed save and render inline/alert validation details.

## Migration Plan

1. UI-first rollout in `RulesEngine.UI` guarded by existing API contracts.
2. If API deltas are required for validation/result payloads, ship additive API changes before enabling corresponding UI actions.
3. Add/update automated tests:
- UI/component tests for Home grid load, modal create/edit behavior, and Rules row expansion.
- Integration/API tests for workflow version activation/enabled constraints and rule expression validation responses.
4. Rollout with feature verification checklist:
- Home is default page.
- Create/edit workflow refreshes grid.
- Version selection + activation behaves correctly.
- Rules nested grid and rule edit validation feedback works.

Rollback:
- Revert UI routes/components to prior page(s) without schema rollback.
- If API deltas were added, keep additive endpoints/contracts in place or soft-disable UI usage.

## Open Questions

- Which existing endpoint should be authoritative for pre-save rule expression validation (dedicated validate endpoint vs create/update response validation)?
- Should workflow name edits create a new version or update metadata on selected version only?
- In Home edit modal, should enable/disable action be restricted to currently active selected version only at UI level (in addition to backend enforcement)?
- Should rules nested grid default to active-only revisions or latest-per-rule view for operators?
