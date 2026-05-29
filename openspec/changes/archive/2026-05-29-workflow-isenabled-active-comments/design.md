## Context

Workflow revisioning currently enforces a single active version per workflow identity but does not distinguish operational enablement from activation state. The requested change introduces an explicit workflow-level `IsEnabled` flag and `Comments` metadata while preserving version history behavior and active-version invariants.

This affects API contracts, persistence schema, domain validation, and query filtering. Existing consumers rely on active-version retrieval and version history endpoints, so compatibility and clear default behavior are required.

Constraints:
- Activation invariants (single active version per workflow identity) remain authoritative.
- Only active versions are eligible for enable/disable transitions.
- API list/read behavior must remain deterministic when enablement filter is omitted.
- Comments must be bounded to 4,000 characters.

## Goals / Non-Goals

**Goals:**
- Add workflow-level `IsEnabled` state across model, storage, and API contracts.
- Enforce that only active versions can be enabled/disabled.
- Enforce at most one active+enabled version per workflow identity.
- Add `Comments` field with max 4,000 characters and persistence mapping.
- Add API filtering for enabled-only, disabled-only, and unfiltered modes.
- Preserve existing version-history semantics and backward-compatible defaults.

**Non-Goals:**
- Redesign workflow activation flows or rule-level activation semantics.
- Introduce soft-delete or archival semantics for workflows.
- Change workflow execution engine internals beyond enablement gating/selection.
- Introduce free-form rich text or comment history tracking.

## Decisions

1. Persist `IsEnabled` and `Comments` on workflow revisions.
- Rationale: enablement and comments are revision-specific metadata and should be traceable historically.
- Alternative considered: storing enablement on workflow identity only. Rejected because it loses revision-level auditability and conflicts with versioned reads.

2. Keep activation and enablement as distinct states with validation coupling.
- Rationale: activation selects canonical revision, enablement controls operability. Coupling rule is enforced: only active revision can toggle enablement.
- Alternative considered: implicit enablement=active. Rejected because the requirement explicitly needs temporary disablement of active revisions.

3. Enforce uniqueness invariant per workflow identity for active+enabled revision.
- Rationale: prevents ambiguous runtime selection and keeps deterministic execution/listing behavior.
- Alternative considered: allowing multiple enabled revisions if only one active. Rejected as redundant and error-prone during concurrent updates.

4. Extend list/query endpoints with nullable enablement filter parameter.
- Rationale: supports enabled-only, disabled-only, and all records without introducing endpoint sprawl.
- Alternative considered: separate endpoints for enabled and disabled. Rejected due to API surface duplication.

5. Implement `Comments` length validation at both API/domain and DB schema levels.
- Rationale: defense in depth and predictable error handling.
- Alternative considered: API-only validation. Rejected because direct persistence paths could bypass it.

## Risks / Trade-offs

- [Concurrent enable/disable and activation operations could violate invariants] -> Mitigation: transactional update logic and unique constraint/index strategy where feasible.
- [Existing consumers may assume active implies enabled] -> Mitigation: preserve default behavior when filter is null and document enablement semantics in API contract.
- [Schema migration on existing rows needs default values] -> Mitigation: backfill `IsEnabled` with deterministic default (`true` for active rows, `false` for inactive rows unless business policy dictates otherwise) and `Comments` as null/empty.
- [Endpoint filter ambiguity (`null` vs omitted)] -> Mitigation: define explicit nullable boolean semantics and test all three cases.

## Migration Plan

1. Add schema migration for workflow revision storage:
- Add `IsEnabled` boolean, non-null with staged default/backfill.
- Add `Comments` string column with max length 4000.
- Add/adjust constraints and indexes supporting active+enabled invariant.

2. Update domain models, DTOs, mappers, validators, and repositories.
3. Update API request/response contracts and list/query filtering logic.
4. Add/adjust integration and unit tests for invariants and filtering behavior.
5. Deploy migration before or together with app rollout depending on backward compatibility strategy.

Rollback:
- Application rollback can occur independently if backward-compatible reads are maintained.
- Database rollback requires reversible migration script (drop new columns/constraints) only if no dependent code remains; otherwise perform forward-fix.

## Open Questions

- Should active revisions default to enabled during migration for all existing workflows, or should enablement be explicitly set by operators?
- Should `Comments` be nullable or normalized to empty string in API responses?
- Which existing endpoints are authoritative for enable/disable operations (dedicated route vs update payload), and should non-active toggle attempts return 400 or 409?
