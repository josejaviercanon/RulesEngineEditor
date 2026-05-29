## 1. Backend Contracts and Query Projections

- [x] 1.1 Add/confirm workflow list projection fields used by Rules page to expose `ActiveVersion` and `LastVersion` independently.
- [x] 1.2 Update Rules page workflow-details rules query path to return active-only rule rows (one row per `RuleGuidId`) with `GuidId`, `Name`, `ActiveVersion`, and `LastVersion` metadata.
- [x] 1.3 Ensure API DTO/response mappings in `RulesEngine.API` include the new/required version metadata fields consumed by Rules page grids.

## 2. Rule Activation Invariant Enforcement

- [x] 2.1 Implement/adjust backend rule activation update flow so changing active version from edit-save deactivates the previous active revision before activating the selected revision.
- [x] 2.2 Wrap rule activation switch for a single `RuleGuidId` in a transactional/invariant-preserving operation boundary.
- [x] 2.3 Add guard and validation handling for activation requests that target invalid or non-associated rule versions.

## 3. Rules Page Grid Updates (RulesEngine.UI)

- [x] 3.1 Update Rules page workflow main data grid to add `Active Version` immediately after `Guid ID`.
- [x] 3.2 Update Rules page workflow-details rules grid to place `Active Version` immediately before `Last Version`.
- [x] 3.3 Bind Rules page nested rules grid to the active-only backend data source and verify no duplicate logical rule rows appear.

## 4. Rule Edit Modal Versioning Parity

- [x] 4.1 Add rule versions list box to the edit-rule modal and load all retained versions for the selected `RuleGuidId`.
- [x] 4.2 Add active-version toggle control to the edit-rule modal with on/off state bound to selected version data.
- [x] 4.3 Implement version-selection change handler to reload expression and all persisted rule properties for the selected version.
- [x] 4.4 Ensure Save updates only the selected version, including active-version changes, while preserving explicit create-new-version behavior.

## 5. Verification and Regression Coverage

- [x] 5.1 Add/update backend tests for active-only rule retrieval and single-active-version invariant enforcement.
- [x] 5.2 Add/update API contract tests for Rules page list payloads containing distinct `ActiveVersion` and `LastVersion` fields.
- [x] 5.3 Add/update UI/component tests (or integration checks) for grid column order and rule edit modal version-switch reload behavior.
- [ ] 5.4 Perform manual end-to-end validation on Rules page: grid columns, active-only details list, modal version switching, and active-version save outcome.