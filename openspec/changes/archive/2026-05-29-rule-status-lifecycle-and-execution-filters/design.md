## Context

Rule definitions currently do not carry an explicit persisted lifecycle status in the backend model. Validation and execution outcomes are returned per request but are not reconciled into a durable per-rule status state that users can control. The requested behavior introduces both user-authoritative transitions (`disabled`, `production`) and system-authoritative transitions (`failed` on compile/runtime failure under defined conditions), which impacts domain logic, API contracts, persistence, and execution/reporting paths.

The change spans multiple modules: domain model, validation/compilation workflows, execution workflows (dry-run and real run), DTO/API contracts, and database schema.

## Goals / Non-Goals

**Goals:**
- Introduce a first-class `RuleStatus` lifecycle with allowed values: `draft`, `failed`, `disabled`, `production`.
- Default new rules to `draft` at create time.
- Permit saving rules with compile/validation errors when status is `draft`.
- Enforce authority boundaries:
  - user can explicitly set `disabled`
  - user can set `production` only if validations pass
  - system sets `failed` on compile/runtime failures when status policy allows.
- Ensure active production rules transition to `failed` when execution fails.
- Add dry-run/test execution status filtering over active workflow version rules (include/exclude status sets).
- Return status transition/reporting metadata so clients can explain outcomes.

**Non-Goals:**
- Adding new status values beyond the four requested.
- Redesigning rule versioning model beyond status-aware behavior.
- Implementing role-based security beyond existing user-authoritative command intent.
- Building UI changes in this change set (API/contract behavior only).

## Decisions

1. Decision: Represent status as a constrained string value in contracts and persisted enum in domain/persistence.
- Rationale: DTO/API remains backward-friendly and explicit (`draft|failed|disabled|production`), while domain and DB can enforce valid values via enum + check constraint.
- Alternatives considered:
  - String everywhere: simpler but weaker invariants.
  - Enum everywhere including API: stronger typing but less interoperable and more brittle for clients.

2. Decision: Apply transition policy through a centralized `RuleStatusPolicy` service in application/core layers.
- Rationale: A single policy engine avoids scattered conditional logic in validators, handlers, and executors.
- Alternatives considered:
  - Handler-local logic in each command: faster to implement but inconsistent and hard to test.
  - DB triggers for transitions: enforces persistence but lacks request-context nuance.

3. Decision: Add explicit status filter parameter for dry-run execution (e.g., `includeStatuses=draft,failed,production`) with default `draft,failed,production`.
- Rationale: Matches requested behavior (exclude disabled by default) and supports selective testing scenarios.
- Alternatives considered:
  - Fixed behavior only (always include all non-disabled): less flexible.
  - Multiple booleans per status: noisier API and harder to evolve.

4. Decision: On compile/runtime failures, transition rules to `failed` only when status is currently `production`; keep `draft` as `draft`; keep `failed` as `failed`; never auto-transition `disabled`.
- Rationale: Aligns with requested semantics and preserves user intent for draft/disabled states.
- Alternatives considered:
  - Always mark any non-disabled as failed: would incorrectly mutate draft authoring workflows.
  - Never mutate status on runtime failures: loses operational signal.

5. Decision: Persist status transitions atomically with execution records for real runs; for dry runs, return transition preview/report without persisting unless explicitly requested by endpoint behavior.
- Rationale: Prevents mismatch between execution evidence and status state while preserving dry-run non-persistence contract.
- Alternatives considered:
  - Persist transitions in dry run: violates dry-run semantics.
  - Never persist runtime transitions: misses requested automatic fail behavior.

## Risks / Trade-offs

- [Risk] Ambiguity between compile-time vs runtime failure sources in transition logic.
  - Mitigation: Include failure source in transition metadata and tests per failure type.
- [Risk] Existing clients may not provide status fields on update/create.
  - Mitigation: Server-side defaulting to `draft` and backward-compatible contract parsing.
- [Risk] Filter semantics could be misunderstood when combined with active version selection.
  - Mitigation: Document precedence: active version selection first, status filter second.
- [Risk] Additional writes during real execution (status transition persistence) may impact throughput.
  - Mitigation: Batch updates per workflow execution and constrain writes to changed rules.

## Migration Plan

1. Add DB migration for rule status column with default `draft` and check constraint to allowed values.
2. Backfill existing rows with `draft` (or mapped value if future migration policy requires).
3. Deploy application code that reads/writes status and applies transition policy.
4. Roll out API changes with non-breaking defaults for missing filter/status inputs.
5. Validate via integration tests and canary execution paths.
6. Rollback strategy: keep migration reversible (drop constraint/column) and gate new behavior behind compatibility checks if rollback required.

## Open Questions

- Should real execution provide an opt-out to prevent automatic production->failed transition for specific maintenance scenarios?
- Should dry-run endpoint support an explicit "simulate-and-persist-status" mode, or must it remain strictly non-persistent?
- Should transition metadata include actor identity for user-initiated status changes in this change, or defer to auditing change?
