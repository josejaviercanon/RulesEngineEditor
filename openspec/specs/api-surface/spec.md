# api-surface Specification

## Purpose
Define the externally visible Minimal API behavior for workflow lifecycle operations, HTTP semantics, and response contract expectations.

## Requirements

### Requirement: Workflow CRUD Endpoints
The system SHALL expose HTTP endpoints for workflow lifecycle operations.

#### Scenario: CRUD operations
- GIVEN the API is running
- WHEN requests are made to /api/workflows (GET, POST, PUT, DELETE)
- THEN the corresponding operations execute with proper HTTP semantics
