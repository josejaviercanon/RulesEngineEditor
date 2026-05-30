## Context

The product already supports workflow/rule editing through the Rules Page and persists workflow/rule JSON payloads. A LogicFlow-oriented visual editing experience is requested to improve authoring and comprehension of rule execution order. This change touches UI (new route and diagram interactions), shared contracts (rule order), backend save/validate flows, and persistence (new rule field).

Constraints:
- Preserve current rule editing semantics and options/types of RulesEngine.
- Keep existing Rules Page workflows operational while adding Visual Editor entry points.
- Ensure all persisted JSON remains load-safe even when saved with validation errors as draft.
- Maintain active-version workflow selection for visual load.

Stakeholders:
- Rule authors need a graph-first editing workflow.
- Backend/API maintainers need deterministic rule ordering and safe persistence.
- QA needs predictable validation and draft-save behaviors.

## Goals / Non-Goals

**Goals:**
- Add a `Visual Editor` UI that initializes as a blank LogicFlow diagram.
- Support workflow load from active versions and auto-load from Rules grid context action.
- Support rule node create/connect/edit with the same property schema as existing rule editor modal.
- Introduce and propagate `ExecuteOrder` numeric field across data model, API, UI, and display sorting.
- Save complete workflow edits by regenerating `WorkflowJson` and each `RuleJson` from the current visual/state model.
- Validate before save and provide user choice to save as draft with warnings or cancel.
- Guarantee JSON syntax correctness on every persisted save path.

**Non-Goals:**
- Replacing the existing Rules Page editor with only visual editing.
- Redesigning existing workflow execution engine semantics beyond deterministic ordering support.
- Implementing advanced graph analytics (cycle scoring, auto-layout optimization, etc.) beyond required ordering/linking behavior.

## Decisions

1. Create a dedicated `Visual Editor` route with shared editing state service
- Decision: Add a new page/component under `RulesEngine.UI` for visual editing; use a shared state model that can be hydrated from loaded workflow DTOs or blank defaults.
- Rationale: Keeps current Rules Page stable while enabling deep visual interactions.
- Alternatives considered:
  - Embed visual canvas into Rules Page tabs: rejected due to increased page complexity and lifecycle coupling.
  - Separate standalone SPA: rejected due to duplicated auth/state/service wiring.

2. Treat `ExecuteOrder` as canonical ordering key for UI and persistence
- Decision: Add `ExecuteOrder` in rule entity/model/DTOs and use it for rule grid sorting and diagram linkage ordering.
- Rationale: Explicit numeric ordering avoids ambiguous order derived only from edge traversal.
- Alternatives considered:
  - Infer order only from graph edges: rejected because edge-only order can be non-deterministic in disconnected or partially linked graphs.
  - Preserve insertion order: rejected as unstable across edits and persistence cycles.

3. Reuse rule property editor schema and validation surface across Rules Page and Visual Editor
- Decision: Use a common modal component/model for rule property editing including `ExecuteOrder`.
- Rationale: Ensures parity and reduces drift in supported rule types/options.
- Alternatives considered:
  - Duplicate modal implementation in Visual Editor: rejected due to maintenance risk.

4. Save pipeline regenerates JSON from normalized in-memory workflow model
- Decision: On save, map visual graph + rule properties to canonical workflow aggregate, then regenerate `WorkflowJson` and per-rule `RuleJson` before persistence.
- Rationale: Prevents stale JSON and guarantees consistency between visual and structured models.
- Alternatives considered:
  - Incremental JSON patching: rejected because patch drift is difficult to reason about and validate.

5. Two-step save validation with draft override while preserving JSON parseability
- Decision: Execute dry-run validation first. If domain validation errors are present, show warning modal with actions: `Save as Draft` or `Cancel`. Save-as-draft remains blocked on JSON serialization/format correctness.
- Rationale: Supports iterative authoring while preventing corrupted persisted payloads.
- Alternatives considered:
  - Hard-block all validation failures: rejected as too restrictive for iterative design workflows.
  - Allow any malformed JSON on draft: rejected due to future load failures.

6. Auto-load workflow context when navigating from Rules grid `Visual Editor` command
- Decision: Add command action with workflow identifier/version in navigation state or query parameters; Visual Editor loads matching workflow on initialization.
- Rationale: Reduces user friction and preserves editing context.
- Alternatives considered:
  - Navigate without context and require manual load every time: rejected as slower and error-prone.

## Risks / Trade-offs

- [Graph/order mismatch between links and `ExecuteOrder`] → Mitigation: normalize links/order during save and show user warnings for conflicting order assignments.
- [Regression risk in existing Rules modal after shared component refactor] → Mitigation: add focused UI/component tests and parity checks for all rule field mappings.
- [Complexity across UI/API/DB migration] → Mitigation: phase work by contract-first changes (DTO/entity/migration), then UI wiring, then save/validate flows.
- [Users saving invalid business logic as drafts repeatedly] → Mitigation: persist validation result metadata/status and surface warning indicators in list/detail views.
- [Potential edge cases in disconnected/cyclic graphs] → Mitigation: explicit validation rules and deterministic fallback ordering using `ExecuteOrder`.

## Migration Plan

1. Add persistence and contract support for `ExecuteOrder` (entity, EF migration, DTOs, mapping profiles).
2. Update API/application save/load pathways to include `ExecuteOrder` and canonical ordering.
3. Refactor rule modal into shared component/model and add `ExecuteOrder` input.
4. Build Visual Editor page with blank initialization, load modal, save action, and LogicFlow node/link interactions.
5. Add Rules grid `Visual Editor` action and navigation auto-load behavior.
6. Implement save-time dry-run validation + draft override dialog + JSON correctness guard.
7. Add/adjust tests (unit/integration/UI) for ordering, serialization, and save decision flow.
8. Rollout:
   - Deploy DB migration first.
   - Deploy API + UI together.
   - Rollback strategy: revert app deployment; if needed, keep `ExecuteOrder` column unused (non-destructive schema addition) until re-release.

## Open Questions

- Should `ExecuteOrder` uniqueness be enforced per workflow at DB level, API validation level, or both?
- For cyclic graph links, should save-as-draft allow persistence with warnings or enforce acyclic structure even for drafts?
- Should draft status be represented as a dedicated workflow/rule status value or only as validation metadata?
- What is the preferred UX for reassigning `ExecuteOrder` when users reorder nodes visually (auto-renumber vs manual only)?
