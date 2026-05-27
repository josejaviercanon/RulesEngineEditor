# infrastructure Specification

## Purpose
Define EF Core persistence behavior, repository responsibilities, and workflow data storage contracts for the infrastructure layer.

## Requirements

### Requirement: Workflow Persistence
The system MUST store workflow definitions as JSON in an EF Core database.

#### Scenario: Save new workflow
- GIVEN a valid workflow JSON and a unique name
- WHEN the save endpoint receives a POST request
- THEN the workflow is persisted in the Workflows table JSON column
- AND a 201 Created response with the workflow ID is returned

#### Scenario: Load existing workflow
- GIVEN a workflow ID that exists in the database
- WHEN the load endpoint receives a GET request
- THEN the system returns the workflow JSON and metadata
