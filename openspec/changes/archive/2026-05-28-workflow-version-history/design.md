## Context

The backend already carries workflow metadata on `WorkflowDto`, `WorkflowRecord`, and `WorkflowEntity`, but the current repository implementation treats `Id` as a single mutable row key. That means updates overwrite history instead of preserving revisions, and there is no backend mechanism to switch between older versions while keeping exactly one active version per workflow identity.

This change affects `RulesEngine.Application`, `RulesEngine.Core`, `RulesEngine.Infrastructure`, and `RulesEngine.API`. The main constraint is to keep the public workflow identity stable while making version history first-class in persistence and API behavior.

## Goals / Non-Goals

**Goals:**
- Preserve every workflow revision for a workflow identity.
- Auto-increment versions per workflow identity.
- Ensure only one revision is active at a time.
- Allow re-activating an older version without deleting history.
- Expose version history and activation through the API.

**Non-Goals:**
- Reworking the RulesEngine expression model itself.
- Introducing a separate workflow family identifier in the public API.
- Changing UI behavior beyond what is required to consume the new backend contracts.

## Decisions

### Keep `WorkflowDto.Id` as the stable workflow identity

**Choice**: Treat the existing `Guid Id` as the workflow identity that groups all revisions for the same workflow.

**Rationale**: The public contract already exposes `Id`, and the user explicitly wants versioning per workflow Guid. Keeping the identity stable avoids a breaking API rename and matches the current command/query shape.

**Alternative considered**: Introduce a separate public workflow-family identifier and a revision identifier. That would make persistence cleaner in isolation, but it would require broader API changes with no user-facing benefit.

### Store revisions as multiple rows keyed by `(Id, Version)`

**Choice**: Refactor persistence so the workflow table stores many rows for the same workflow identity, with version number providing the revision key. The database should enforce uniqueness on `(Id, Version)` and should also enforce at most one active row per `Id`.

**Rationale**: This preserves history with the least amount of contract churn. The code can look up the current active revision by `Id`, list history by `Id`, and activate a specific revision by `Id` plus `Version`.

**Alternative considered**: Overwrite the existing row and store prior versions in a separate audit table. That would complicate read/update logic and make “active version” switching indirect.

### Create/update become version-producing operations

**Choice**: `Create` writes version 1 for a new identity. `Update` appends a new revision with `max(version) + 1` for the identity, keeps prior rows, and marks the new revision active.

**Rationale**: This matches the user request that each new revision increments automatically and that older versions remain available for later reactivation.

**Alternative considered**: Mutate the active row in place and only create new rows for explicit “save as version” actions. That would not satisfy the historical-record requirement.

### Activation is a transactionally coordinated flag swap

**Choice**: Activating version `N` for a workflow identity must run in a single transaction that clears `IsActive` from the other revisions for that identity and sets `IsActive = true` on the selected revision.

**Rationale**: The user asked for exactly one active version at any time. A transaction plus a unique active constraint prevents split-brain active state during concurrent operations.

**Alternative considered**: Lazy activation via read-time filtering. That would not guarantee a single authoritative active revision in the database.

### Keep the active workflow cache keyed by workflow identity

**Choice**: Continue using the existing workflow service cache keyed by workflow `Id`, but refresh it whenever the active version changes.

**Rationale**: Execution should always use the active revision for a workflow identity. Refreshing on create, update, and activation keeps the cache aligned with persistence without adding a version-aware execution API.

**Alternative considered**: Key the execution cache by `Id + Version`. That would force every execution call to know the version explicitly and would not match the requested “current active version” semantics.

## Risks / Trade-offs

- [Risk: PK change on the workflow table] Existing schema currently treats `Id` as a unique row key. → Mitigation: migrate to a versioned key model and backfill current records as version 1 active rows.
- [Risk: Concurrent updates for the same workflow identity] Two writers could compute the same next version. → Mitigation: wrap version allocation and activation in a transaction and enforce a unique `(Id, Version)` constraint.
- [Risk: Ambiguous delete semantics] Deleting the workflow identity could mean deleting one revision or the entire history. → Mitigation: keep delete as identity-level delete unless a separate revision-delete requirement is added later.
- [Risk: Execution cache staleness] Activating an older version without refreshing the cache would run stale rules. → Mitigation: refresh the cache immediately after the active-version swap completes.

## Migration Plan

1. Update the workflow persistence model so the database can store multiple revisions per workflow identity and enforce a single active revision.
2. Backfill existing workflow rows to version 1 and active state.
3. Update repository methods to return the active revision by default and expose version-history and activation operations.
4. Update application handlers and API routes to create new revisions on update, return active revisions on read/list, and activate a selected version explicitly.
5. Refresh the workflow execution cache whenever the active revision changes.
6. Validate the new endpoints and version-switching paths with integration tests.

Rollback: revert the schema and repository changes together. Since the public identity stays the same, rollback mainly restores single-row semantics for the workflow table.

## Open Questions

- Should delete remove the entire workflow history for an identity, or should a future revision-level delete endpoint be added instead?

## Decision Log Updates

- The API exposes `GET /api/workflows/{id}/versions/{version}` so clients can fetch a specific retained revision directly. This supports verification/debugging flows and aligns with the implemented backend endpoint and tests for this change.