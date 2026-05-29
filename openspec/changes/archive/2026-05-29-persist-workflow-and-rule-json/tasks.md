## 1. Data Model and Persistence Schema

- [x] 1.1 Add `WorkflowJson` to workflow domain/entity models and EF configurations in backend projects
- [x] 1.2 Add/confirm `RuleJson` on rule domain/entity models and EF configurations, ensuring JSON includes plain-text `Expression`
- [x] 1.3 Create and review database migration(s) to add/adjust `WorkflowJson` and `RuleJson` columns with safe nullability/default behavior

## 2. DTOs, Contracts, and Mapping

- [x] 2.1 Extend workflow DTO/contracts to include `WorkflowJson` and rule DTO/contracts to include `RuleJson`
- [x] 2.2 Update AutoMapper and manual mapping paths so workflow/rule JSON fields round-trip correctly in create/update/read operations
- [x] 2.3 Implement write-path consistency rules so structured fields and JSON fields are synchronized per revision

## 3. Application and API Flow Updates

- [x] 3.1 Update command/handler/service flows to persist full workflow JSON (`WorkflowName` + `Rules[]`) to `WorkflowJson`
- [x] 3.2 Update rule persistence flow to save per-rule JSON with `Expression` as plain-text inside `RuleJson`
- [x] 3.3 Ensure API endpoints and response shaping expose/consume new JSON fields without breaking existing clients

## 4. Migration and Backfill Safety

- [x] 4.1 Add migration-safe fallback handling for legacy records that do not yet have JSON payloads
- [x] 4.2 Implement backfill process (or startup job/script) to populate `WorkflowJson` and `RuleJson` from existing structured data
- [x] 4.3 Document rollback and deployment sequence for phased rollout of schema + application updates

## 5. Validation and Tests

- [x] 5.1 Add unit tests for workflow/rule mapping to verify `WorkflowJson` and `RuleJson` serialization fidelity
- [x] 5.2 Add integration tests for create/update/read operations validating JSON/structured field consistency in DB
- [x] 5.3 Add regression tests that confirm rule `Expression` remains plain text in `RuleJson` and supports RulesEngine-compatible payloads
