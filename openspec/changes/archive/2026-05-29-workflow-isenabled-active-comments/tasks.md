## 1. Domain and DTO updates

- [x] 1.1 Add workflow-level `IsEnabled` and `Comments` fields to domain models and DTO contracts with `Comments` max length 4000 validation.
- [x] 1.2 Update mapping profiles and conversion logic so `IsEnabled` and `Comments` round-trip between API DTOs, application models, and persistence models.
- [x] 1.3 Update command/query validators to reject invalid `Comments` length and reject enable/disable requests for non-active workflow revisions.

## 2. Persistence and migration

- [x] 2.1 Add EF Core/entity configuration for `IsEnabled` and `Comments` (`character varying(4000)`), including required defaults/backfill behavior.
- [x] 2.2 Create and verify database migration to add new columns and enforce active+enabled invariants per workflow identity.
- [x] 2.3 Update repositories/queries to read and persist `IsEnabled` and `Comments` across versioned workflow operations.

## 3. API behavior and filtering

- [x] 3.1 Update workflow endpoints that return workflow data to include `IsEnabled` and `Comments` in response payloads.
- [x] 3.2 Add nullable `isEnabled` query parameter handling to list/read endpoints: true=enabled-only, false=disabled-only, null/omitted=all.
- [x] 3.3 Update enable/disable API operations to enforce active-version-only toggling and consistent error responses for invalid transitions.

## 4. Tests and verification

- [x] 4.1 Add unit tests for lifecycle invariants: only active revisions can be toggled, and only one active+enabled revision exists per workflow identity.
- [x] 4.2 Add API/integration tests for `isEnabled` filtering modes and payload coverage of `IsEnabled` and `Comments`.
- [x] 4.3 Add persistence tests for migration defaults, column constraints, and round-trip behavior for `Comments` length boundaries.
- [x] 4.4 Update OpenAPI/endpoint documentation and changelog notes for new fields and filter semantics.
