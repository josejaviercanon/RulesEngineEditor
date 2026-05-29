## Why

The current persistence model stores workflow and rule fields in a fragmented way that can lose fidelity with the canonical RulesEngine workflow document shape. We need a durable JSON source of truth so backend processing, model mapping, and database storage preserve the exact workflow plus rule expressions as authored.

## What Changes

- Add `WorkflowJson` to the `Workflow` entity/table to persist the full RulesEngine workflow JSON payload, including all rules.
- Persist rule JSON in the `Rule` entity/table using `RuleJson`, where each rule JSON includes the plain-text `Expression` representation.
- Update backend contracts, mappings, and persistence flow so create/update/read operations maintain consistency between structured fields and stored JSON.
- Add migration updates for database schema and constraints needed for the new JSON fields.
- Ensure serialization format aligns with RulesEngine workflow format documented at: https://microsoft.github.io/RulesEngine/#create-a-workflow-file-with-rules.

## Capabilities

### New Capabilities
- `workflow-json-persistence`: Persist full workflow JSON and per-rule JSON representations as first-class data in backend models and storage.

### Modified Capabilities
- `workflow-full-dto-model`: Extend workflow and rule DTO/model requirements to include canonical JSON payload fields (`WorkflowJson`, `RuleJson`) and expression fidelity.

## Impact

- Affected backend projects: `src/RulesEngine.API`, `src/RulesEngine.Application`, `src/RulesEngine.Core`, `src/RulesEngine.Infrastructure`.
- Affected storage: workflow and rule tables require schema changes and migration.
- Affected mappings and contracts: workflow/rule DTOs, entity mappings, and API request/response models.
- Potential compatibility consideration: existing records without JSON fields need migration-safe defaults/backfill behavior.
