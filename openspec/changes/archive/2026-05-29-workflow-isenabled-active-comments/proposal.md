## Why

Workflow versions currently track activation state but do not provide a separate enablement state for operational control. We need explicit enable/disable behavior on active workflow versions, plus richer metadata, so API consumers can safely control runtime eligibility without breaking version history semantics.

## What Changes

- Add workflow-level `IsEnabled` state that is persisted and exposed in backend models, DTOs, and API responses.
- Enforce invariants: only one version per workflow identity can be both active and enabled, and only the currently active version can be enabled or disabled.
- Add validation to reject enable/disable operations against non-active workflow versions.
- Update workflow API endpoints to support filtering by enablement state: enabled-only, disabled-only, or all when filter is omitted/null.
- Add workflow `Comments` field with max length 4,000 characters, stored as PostgreSQL `nvarchar(4000)` equivalent mapping (`character varying(4000)`) in persistence.
- Update data access and persistence schema to include `IsEnabled` and `Comments` in workflow storage and versioned reads.

## Capabilities

### New Capabilities
- `workflow-enablement-controls`: Define enable/disable state semantics and API filtering behavior for workflow versions.

### Modified Capabilities
- `workflow-lifecycle`: Extend lifecycle operations with enablement transition validation and active-version-only toggling rules.
- `workflow-full-dto-model`: Extend workflow DTO contracts to include `IsEnabled` and `Comments` fields with validation constraints.

## Impact

- Affected backend: workflow domain models, validators, command/query handlers, repositories, and EF Core mappings/migrations.
- Affected API surface: workflow read/list and enable/disable related endpoints and request query parameters.
- Affected database: workflow persistence schema and migration scripts for `IsEnabled` and `Comments` columns with constraints.
- Affected tests: lifecycle validation tests, API filtering tests, and persistence mapping/migration tests.
