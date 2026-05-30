# infrastructure Specification

## Purpose
Define EF Core persistence behavior, repository responsibilities, and workflow data storage contracts for the infrastructure layer.

## Requirements

### Requirement: Workflow Persistence
The system MUST store workflow definitions as JSON in an EF Core database and MUST support create/read/update/delete operations through infrastructure repositories used by application handlers.

#### Scenario: Save new workflow
- **WHEN** a valid workflow JSON and unique workflow name are submitted through backend create flow
- **THEN** the workflow is persisted in the workflows store and created identity is returned

#### Scenario: Load existing workflow
- **WHEN** a workflow identifier that exists in persistence is requested
- **THEN** the system returns persisted workflow JSON and lifecycle metadata

#### Scenario: Update existing workflow definition
- **WHEN** an existing workflow is updated through backend update flow
- **THEN** persisted workflow definition fields are replaced with updated values

### Requirement: Execution State Persistence
The system MUST persist non-dry-run workflow execution outcomes in infrastructure storage, including execution identifier, workflow identifier, timestamp, success flag, and serialized result payload.

#### Scenario: Persist execution result for real execution
- **WHEN** workflow execution completes with dryRun=false
- **THEN** an execution state record is stored and linked to the workflow identifier

#### Scenario: Do not persist execution result for dry-run execution
- **WHEN** workflow execution completes with dryRun=true
- **THEN** no execution state record is written
