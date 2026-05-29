## Context

Rules are currently persisted without first-class version lineage per stable rule Guid, making rollback and audit difficult. Workflow retrieval can also surface duplicate rows when multiple revisions of logically identical rules are returned together. The backend must support deterministic version promotion/demotion while preserving history and enforcing a single active rule revision for each rule Guid identity.

This change spans API contracts, application logic, domain model, data access, and database schema/migrations. It must be deployable without data loss and with a clear rollback strategy.

## Goals / Non-Goals

**Goals:**
- Persist rule revisions under a stable `RuleGuidId` identity with monotonic integer `Version` values.
- Enforce exactly one active/enabled version per `RuleGuidId`.
- Support activating any existing version by switching active flags atomically.
- Persist workflow-to-rule membership as a rule collection model for reliable query shaping.
- Provide backend query modes:
  - active-only per workflow (execution-safe, no duplicates)
  - latest-only per workflow rule identity (management-safe, no duplicates)
  - full history on explicit request
- Keep workflow and rule history auditable.

**Non-Goals:**
- Frontend UX redesign for history browsing.
- Rule expression language changes.
- Multi-active A/B rule execution for the same `RuleGuidId`.
- Cross-workflow global version synchronization.

## Decisions

1. Data model split: identity + revision
- Decision: model rules as revision records keyed by immutable row id, grouped by `RuleGuidId` and `Version`.
- Rationale: separates logical identity from physical revisions and enables deterministic history.
- Alternatives considered:
  - Overwrite-in-place rows (rejected: loses history and prevents safe rollback).
  - Workflow-level only versioning (rejected: cannot target individual rule rollback).

2. Single-active enforcement at database and service layers
- Decision: enforce one active revision per `RuleGuidId` using a filtered unique index/constraint and transactional application logic.
- Rationale: DB-level protection prevents race-induced dual-active states; service-level transactions preserve atomic switches.
- Alternatives considered:
  - Service-only checks (rejected: vulnerable under concurrent writes).
  - Trigger-based enforcement (rejected: less portable and harder to test).

3. Version assignment strategy
- Decision: assign new `Version` as `MAX(version)+1` scoped by `RuleGuidId` within a transaction/locking boundary.
- Rationale: monotonic and human-readable sequence; avoids gaps caused by client-side counters.
- Alternatives considered:
  - Global sequence (rejected: version meaning becomes ambiguous per rule identity).
  - Timestamp-based versioning (rejected: less ergonomic for explicit activation like 8/9/10).

4. Workflow rule-collection model
- Decision: introduce workflow collection storage mapping workflow identity to logical rule identities (`RuleGuidId`) and revision references for historical tracing.
- Rationale: establishes authoritative rule membership and supports duplicate-free projections.
- Alternatives considered:
  - Infer membership from latest rule table scans (rejected: brittle and expensive for history queries).

5. Query contract modes
- Decision: expose explicit backend query modes/flags:
  - `ActiveOnly`: one active revision per `RuleGuidId` in a workflow
  - `LatestPerRule`: highest version per `RuleGuidId` regardless of active flag
  - `IncludeHistory`: all revisions for all rule identities in workflow
- Rationale: prevents implicit/ambiguous query behavior and keeps callers intentional.
- Alternatives considered:
  - Single endpoint with implicit defaults only (rejected: caller confusion and regression risk).

6. Activation semantics
- Decision: activating an old version (e.g., 8 when 10 is active) only toggles active flags; no new revision row is created.
- Rationale: preserves immutable revision lineage and maps exactly to operator intent.
- Alternatives considered:
  - Clone activated version into new latest version (rejected: distorts history and inflates revisions).

## Risks / Trade-offs

- [Concurrent activation requests could conflict] -> Mitigation: transaction boundary around deactivate/activate, optimistic concurrency token, and DB unique active constraint.
- [Migration complexity for existing data without versions] -> Mitigation: one-time backfill assigns `Version=1` and active flag for current records before enabling strict constraints.
- [Query performance regressions with latest-per-group filtering] -> Mitigation: composite indexes on `(WorkflowId, RuleGuidId, Version DESC)` and `(RuleGuidId, IsActive)`.
- [API behavior changes for existing clients] -> Mitigation: preserve default response mode compatible with current behavior where possible and document new query flags.
- [Potential stale cache after activation switch] -> Mitigation: invalidate workflow/rule caches on successful activation transaction commit.

## Migration Plan

1. Introduce schema changes:
- Add rule revision columns/entities (`RuleGuidId`, `Version`, `IsActive`, audit metadata).
- Add workflow rule-collection tables and FK constraints.
- Add unique/index constraints for single-active and latest-query performance.

2. Backfill existing data:
- For each existing logical rule, set `RuleGuidId` (or derive from current Guid), `Version=1`, `IsActive=true`.
- Populate workflow rule-collection memberships from current workflow-rule relationships.

3. Deploy application changes:
- Repositories/services implement version creation, activation switching, and query modes.
- API endpoints/contracts updated for activation and retrieval filters.

4. Verify and cut over:
- Run integration tests for create/update/activate/query scenarios.
- Validate no dual-active rows and no duplicate active/latest responses per workflow.

5. Rollback strategy:
- If application rollout fails before data migration finalization, roll back binaries and keep migration transactionally reversible where supported.
- If post-cutover issue appears, disable new endpoints/flags and revert to previous binary while retaining additive schema fields; avoid destructive migration rollback unless data corruption risk is confirmed.

## Open Questions

- Should activation endpoint be idempotent when requested version is already active (recommended: yes, no-op success)?
- Do we need soft-delete support at revision level, or is inactive + audit trail sufficient?
- Should `LatestPerRule` include inactive drafts that were never active, or only published revisions?
- Do we expose effective activation timestamps in API responses for audit/reporting filters?
