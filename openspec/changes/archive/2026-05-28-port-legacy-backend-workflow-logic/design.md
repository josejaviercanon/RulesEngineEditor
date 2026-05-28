## Context

The current solution has a clean-architecture project layout (`RulesEngine.API`, `RulesEngine.Application`, `RulesEngine.Core`, `RulesEngine.Infrastructure`) but significant workflow behavior remains either shell-level or UI-coupled compared with `Legacy/src/RulesEngineEditor`.

Legacy behavior combines workflow editing, JSON normalization, input parsing, rules engine invocation, and error localization inside Blazor page/component code and `WorkflowService` events. The new backend already has partial workflow endpoints, schema validation, execution recording, and repositories, but the application layer orchestration and parity guarantees are incomplete.

Constraints:
- Preserve existing API contracts where possible and evolve in a backward-compatible way.
- Use legacy implementation only as behavior baseline; do not reintroduce UI coupling into backend layers.
- Keep rule execution grounded on `RulesEngine` library semantics and existing schema validation strategy.
- Ensure deterministic behavior through tests before deprecating legacy parity checks.

Stakeholders:
- Backend/API developers implementing clean-architecture workflow lifecycle.
- UI/editor developers relying on stable backend behavior.
- QA/integration owners validating migration parity.

## Goals / Non-Goals

**Goals:**
- Centralize workflow lifecycle logic (create/list/get/update/delete/validate/execute) in backend services and handlers.
- Formalize backend input normalization and validation paths currently implied by legacy UI code.
- Persist workflow execution outcomes consistently (dry-run vs persisted execution).
- Provide explicit error contracts and schema-version-aware validation responses.
- Add parity-focused tests spanning core service, application orchestration, repository behavior, and API endpoints.

**Non-Goals:**
- Rebuilding legacy Blazor UI interactions in the backend.
- Introducing new rule authoring UX features beyond current legacy behavior baseline.
- Replacing the underlying `RulesEngine` library.
- Designing long-term multi-tenant or distributed execution infrastructure in this change.

## Decisions

1. Backend lifecycle orchestration belongs in application handlers, not API endpoints.
- Decision: Move endpoint-level orchestration into command/query handlers and application services that coordinate repositories, schema validator, and core execution service.
- Rationale: Keeps endpoints thin and testable, avoids duplication, and aligns with clean architecture boundaries.
- Alternative considered: Keep orchestration in minimal API handlers. Rejected because current handlers become hard to reuse/test as complexity grows.

2. Treat legacy `WorkflowService` behavior as parity specification, not implementation model.
- Decision: Extract parity requirements (ordering, validation timing, failure reporting, execution mapping) and implement them via explicit backend contracts/services.
- Rationale: Legacy code is UI-stateful/event-driven; direct port would violate backend boundaries.
- Alternative considered: Wrap legacy service in backend adapter. Rejected due to coupling, hidden state, and maintenance risk.

3. Standardize workflow validation and execution result contracts.
- Decision: Define a consistent backend response model for validation errors and execution outcomes (including dry-run semantics and persisted execution IDs).
- Rationale: Clients require predictable shape; current partial implementation is close but not fully parity-defined.
- Alternative considered: Return raw `RulesEngine` types directly. Rejected to avoid leaking engine internals and reduce contract fragility.

4. Preserve repository abstraction and enrich persistence via infrastructure entities.
- Decision: Continue using `IWorkflowRepository` and `RulesEngineEditorDbContext`, adding missing fields/queries and execution-state persistence where needed.
- Rationale: Maintains architecture intent and allows test doubles and integration coverage.
- Alternative considered: Bypass repository abstraction from handlers. Rejected due to tighter coupling and harder tests.

5. Use parity-driven incremental migration with feature-level tests.
- Decision: Add tests first/alongside implementation for each lifecycle slice (CRUD, validate, execute, error paths).
- Rationale: Prevents behavioral drift from legacy baseline and protects future refactors.
- Alternative considered: Full implementation then tests. Rejected due to high regression risk.

## Risks / Trade-offs

- [Parity interpretation drift] Legacy behavior is implicit in UI code and may be ambiguous -> Mitigation: codify behavior in spec scenarios and executable tests before finalizing handlers.
- [Contract break risk] Tightening validation/execution responses could impact consumers -> Mitigation: keep backward-compatible fields and document any additive changes.
- [Performance overhead] Additional validation and serialization steps can increase request latency -> Mitigation: avoid repeated deserialization; benchmark execute endpoint hot path.
- [Persistence mismatch] Existing schema/entities may not capture all legacy semantics -> Mitigation: include migration-safe entity updates and integration tests against real DbContext provider.
- [Scope expansion] Attempting full legacy feature parity can balloon into UI concerns -> Mitigation: enforce non-goals and only port backend-relevant logic.

## Migration Plan

1. Baseline: identify and map legacy backend-relevant behaviors to explicit requirements/spec scenarios.
2. Implement core/application orchestration for lifecycle slices behind existing API routes.
3. Expand infrastructure persistence and repository methods needed for parity behavior.
4. Update API contract mapping and error handling for validation/execution consistency.
5. Add/expand automated tests per slice (unit + integration) and run full backend build/test pipeline.
6. Rollout: ship behind normal backend deployment path; no runtime feature flag required unless contract-risk emerges.

Rollback strategy:
- Revert to prior API/application implementation commit.
- Preserve data compatibility by using additive schema/entity changes where possible.
- If persistence changes are introduced, include reversible migration or no-op backward compatibility mapping.

## Open Questions

- Should execution history retention include configurable limits/TTL in this change, or remain unbounded for now?
- Do we need optimistic concurrency/version conflict semantics for workflow updates in this phase?
- Should validate and execute endpoints accept both persisted workflow IDs and ad-hoc inline workflow payloads, or keep ID-based execution only?
