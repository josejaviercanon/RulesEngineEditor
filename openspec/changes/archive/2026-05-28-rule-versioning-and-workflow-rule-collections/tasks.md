## 1. Domain and Data Model

- [x] 1.1 Add rule revision model fields/entities for `RuleGuidId`, `Version`, and `IsActive` in core/domain and persistence mappings.
- [x] 1.2 Add workflow rule-collection model/table(s) to persist workflow-to-`RuleGuidId` membership.
- [x] 1.3 Add DTO updates so rule payloads include `RuleGuidId`, `Version`, and `IsActive` with retrieval mode support.

## 2. Database Migrations and Constraints

- [x] 2.1 Create migration(s) to add rule revision columns/tables and workflow rule-collection tables with foreign keys.
- [x] 2.2 Add constraints/indexes for single-active-per-`RuleGuidId` enforcement and latest-per-rule query performance.
- [x] 2.3 Implement data backfill migration logic to initialize existing rows with `Version = 1`, `IsActive = true`, and workflow rule memberships.

## 3. Application Logic

- [x] 3.1 Implement rule revision creation flow that assigns `Version = MAX(version)+1` scoped to `RuleGuidId`.
- [x] 3.2 Implement atomic activation flow that toggles active flags for a `RuleGuidId` without creating a new revision.
- [x] 3.3 Implement workflow rule retrieval modes: `ActiveOnly`, `LatestPerRule`, and `IncludeHistory`.
- [x] 3.4 Ensure concurrency handling for competing activation requests and surface deterministic outcomes.

## 4. API Surface

- [x] 4.1 Add/extend endpoints to list versions by `RuleGuidId` and activate a specific rule version.
- [x] 4.2 Extend workflow query endpoints to accept retrieval mode parameters and return duplicate-free results by mode.
- [x] 4.3 Update request/response contracts, validation, and error semantics for new version-management operations.

## 5. Testing and Verification

- [x] 5.1 Add unit tests for version increment rules, single-active enforcement, and activation of historical versions.
- [x] 5.2 Add integration tests for migration backfill correctness and API flows (list versions, activate version, query modes).
- [x] 5.3 Add regression tests to confirm active-only and latest-per-rule workflow responses never contain duplicate logical rules.
- [x] 5.4 Run solution build/tests and verify migration applies cleanly on development database.

## 6. Documentation and Operational Readiness

- [x] 6.1 Document backend versioning behavior, activation semantics, and workflow retrieval modes in API docs/changelog.
- [x] 6.2 Document rollout and rollback runbook steps for schema migration and binary deployment.
