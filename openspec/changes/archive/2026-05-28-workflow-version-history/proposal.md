## Why

Workflows currently behave like a single mutable record, which makes it hard to preserve history, compare prior versions, or safely roll back to an older definition. The backend needs versioned workflow records so users can keep every revision, switch the active version deterministically, and avoid overwriting the prior state when they promote a different version.

## What Changes

- Persist each workflow revision as a versioned record instead of treating the workflow as a single mutable snapshot.
- Auto-increment the workflow version number for each new revision of the same workflow identity.
- Ensure only one version is active for a given workflow identity at a time.
- Support re-activating an older version without rewriting the historical versions.
- Preserve historical versions so the user can inspect or restore previous workflow definitions later.
- Update backend read/write behavior so create, update, list, and activation flows all understand versioned workflow history.
- Add the API surface needed to list versions and activate a specific version explicitly.

## Capabilities

### New Capabilities
- `workflow-version-history`: versioned persistence for workflows, including historical revisions, version increments, and explicit active-version switching.

### Modified Capabilities
- `workflow-lifecycle`: create, update, read, and list semantics change to work with versioned workflow history and a single active version per workflow identity.
- `api-surface`: HTTP contracts gain version-history and active-version management endpoints, and workflow responses must surface version metadata consistently.

## Impact

- `RulesEngine.Application`: workflow commands, handlers, and DTO mapping need to distinguish workflow identity from version identity.
- `RulesEngine.Infrastructure`: persistence and repository logic must store multiple versions per workflow identity and manage active-state transitions.
- `RulesEngine.API`: endpoints need routes for listing workflow versions and activating a chosen version.
- `RulesEngine.Tests`: integration coverage should verify version increments, historical retention, and activation switching.