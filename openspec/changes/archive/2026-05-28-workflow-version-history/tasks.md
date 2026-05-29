## 1. Persistence Model

- [x] 1.1 Refactor the workflow persistence model so `Id` represents the workflow identity and `Version` represents the revision number, with database rules that allow multiple versions per identity and only one active row at a time.
- [x] 1.2 Add a migration and backfill path that preserves existing workflows as version 1 active revisions.

## 2. Repository and Cache Behavior

- [x] 2.1 Update `IWorkflowRepository` and `WorkflowRepository` to create new revisions, return the active revision by identity, list all revisions for an identity, and activate a specific version transactionally.
- [x] 2.2 Update workflow cache refresh logic so create, update, and version-activation flows keep the execution cache aligned with the active revision.

## 3. Application Commands and API Routes

- [x] 3.1 Update create, update, get, and list workflow handlers so they work with versioned history and return the active revision for a workflow identity.
- [x] 3.2 Add application commands and handlers for listing workflow versions and activating a specific version.
- [x] 3.3 Add or update API contracts and routes for `GET /api/workflows/{id}/versions` and `POST /api/workflows/{id}/versions/{version}/activate`.

## 4. Verification

- [x] 4.1 Add integration tests that prove create/update increments versions, preserves prior revisions, and returns the active revision on read/list.
- [x] 4.2 Add integration tests that prove activating an older version flips the active flag and keeps the history intact.
- [x] 4.3 Run backend build and test coverage for the touched projects to confirm the versioned workflow flow is stable.