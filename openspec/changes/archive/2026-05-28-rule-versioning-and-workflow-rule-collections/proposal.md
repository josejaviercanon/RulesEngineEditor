## Why

The current backend does not provide version history per rule identity, so operators cannot safely switch active logic to a previous known-good rule revision without rewriting data. This change is needed now to support auditable rule evolution, deterministic rollback, and consistent workflow rule retrieval without duplicates.

## What Changes

- Add backend rule versioning per stable rule Guid identity with an auto-increment integer version sequence.
- Enforce one and only one active/enabled version at a time for each rule Guid identity.
- Add backend operations to activate any existing version (for example switch active version from 10 to 8) by changing persisted active flags instead of creating a new version.
- Introduce workflow rule-collection persistence so each workflow stores the set of rule identities it owns and resolves active rule versions from that set.
- Add query behavior for workflow rules:
  - Return only active versions per workflow rule identity for execution paths.
  - Return latest version per rule identity (active or inactive) for management/listing paths to avoid duplicate rows.
  - Optionally return full history when explicitly requested.
- Update API contracts, application handlers, persistence mappings, and database schema/migrations to support the above behaviors.

## Capabilities

### New Capabilities
- `rule-versioning-and-activation`: Per-rule Guid version history with monotonic integer versions, single-active enforcement, and explicit version activation.
- `workflow-rule-collections`: Workflow-level persistence of associated rule identities and query projections for active-only, latest-only, and full-history views.

### Modified Capabilities
- `workflow-version-history`: Extend version history semantics from workflow scope to include rule-level activation/rollback behavior and retrieval guarantees.
- `api-surface`: Add and adjust endpoints/contracts for version activation, latest-version retrieval, and filtered workflow rule queries.
- `workflow-full-dto-model`: Expand DTOs to represent rule identity, version number, active flag, and workflow rule-collection projections.

## Impact

- Affected code: `src/RulesEngine.API`, `src/RulesEngine.Application`, `src/RulesEngine.Core`, `src/RulesEngine.Infrastructure`, and integration tests in `src/RulesEngine.Tests`.
- Affected data model: new/updated tables for rule versions and workflow-to-rule collection mappings, plus indexes/constraints for version sequencing and single-active enforcement.
- Affected APIs: workflow and rule management/query endpoints and their response contracts.
- Affected operations: migration and seed behavior for existing rules/workflows to initialize version numbers and active flags.
