# CHANGELOG

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added
- Backend rule-level versioning with stable `RuleGuidId`, monotonic integer `Version`, and `IsActive` switching support.
- Workflow rule collection persistence (`workflow_rules`) to bind workflow revisions to rule identities.
- API query modes for workflow reads: `ActiveOnly`, `LatestPerRule`, and `IncludeHistory`.
- Rule version endpoints under workflows:
	- `GET /api/workflows/{id}/rules/{ruleGuidId}/versions`
	- `POST /api/workflows/{id}/rules/{ruleGuidId}/versions/{version}/activate`

### Changed
- Workflow query/execute projections now resolve rule payloads from persisted rule revisions instead of only raw stored workflow JSON.
- EF Core schema includes rule identity/active-version constraints and migration backfill logic for existing workflows.

### Operational Notes
- Migration `20260529003823_RuleVersioningWorkflowCollections` was validated against development database `GamificationFlow_DEV`.

## [1.2.0] - 6-27-2022
- UI Makeover + bugfixes

## [1.1.2] - 6-9-2022
- Cleanup

## [1.1.0] - 1-10-2021

### Changed
- Updated to latest Rules Engine (3.4.0)


## [1.0.10] - 24-09-2021

### Changed
- Grid line alignment
- Bugfixes

### Added
- WF, Input Delete
- Full EF example in Server
- Advance input scenarios (nested + date time)

## [1.0.9] - 21-09-2021

### Changed
- Inputs on tab fixed
- Rule enabled toggle bugfix
- Removed JsonStringEnumConverter until WASM PWA supported
- Workflow/Input toggler now based on DIV hidden toggle

### Added
- InputRule is now InputRuleName (former deprecated as Obsolete until next ver)
- InputRule.Parameter is now Parameters (former deprecated as Obsolete until next ver)
- Parameter Add assist

## [1.0.0] - 14-09-2021

### Added
- The first version of the NuGet
