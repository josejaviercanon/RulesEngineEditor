## 1. Data Model and Persistence

- [x] 1.1 Add rule status domain type (`draft|failed|disabled|production`) and defaulting rules for new rule creation.
- [x] 1.2 Extend persistence entities/mappings to store rule status and add DB constraint/default migration (`draft` default).
- [x] 1.3 Backfill existing rule records with `draft` status and verify migration rollback path.

## 2. Application Policy and Validation

- [x] 2.1 Implement centralized status transition policy service for user-initiated and system-initiated transitions.
- [x] 2.2 Update create/update/status-change handlers to enforce authority rules (`disabled` user-only, `production` user-only + validations pass).
- [x] 2.3 Update compile/validate flow to return rule-level status transition metadata while preserving non-persistence behavior.

## 3. Execution Semantics and Filtering

- [x] 3.1 Extend execute command/API to accept status filter input (`IncludeStatuses`) with default `draft,failed,production` and disabled exclusion.
- [x] 3.2 Update rule selection logic to apply active-version selection first, then status filtering.
- [x] 3.3 Implement runtime transition behavior: `production` failures persist to `failed`; `draft` and `failed` remain unchanged; `disabled` excluded.
- [x] 3.4 Include per-rule status-before/status-after/transition-reason metadata in execute responses.

## 4. API Contracts and Mapping

- [x] 4.1 Extend `RuleDto` and related contracts with `Status` and validation for allowed values.
- [x] 4.2 Update API endpoint contracts/documentation for create/update/validate/execute to reflect status lifecycle and filtering semantics.
- [x] 4.3 Update AutoMapper and serializers to round-trip status fields across DTO/domain/persistence models.

## 5. Verification and Regression Coverage

- [x] 5.1 Add unit tests for status policy transitions (compile/runtime failures, authority restrictions, defaulting).
- [x] 5.2 Add integration tests for validate endpoint status metadata and non-persistence guarantees.
- [x] 5.3 Add integration tests for execute filtering (`failed` only, `draft` only, `production` only, default set) and disabled exclusion.
- [x] 5.4 Add integration tests for real execution persistence of production->failed transitions and dry-run non-persistence behavior.
- [x] 5.5 Run build/tests and capture any required follow-up tasks from failures.
