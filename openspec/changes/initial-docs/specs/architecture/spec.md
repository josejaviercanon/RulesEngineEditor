## ADDED Requirements

### Requirement: Core Layer Boundary
The system SHALL isolate domain entities, RulesEngine wrapper logic, and schema validation policies inside the core layer.

#### Scenario: Core layer dependency direction
- **GIVEN** the solution is built
- **WHEN** project references are inspected
- **THEN** the core layer has no dependency on infrastructure, api, or ui projects

### Requirement: Shared Editor Layer Responsibility
The system SHALL place shared editor validation utilities and test-runner helper logic in the shared-editor layer for reuse by ui and application layers.

#### Scenario: Shared editor reuse
- **GIVEN** both ui and application require workflow validation helpers
- **WHEN** shared logic is referenced
- **THEN** both layers consume the same shared-editor abstractions without duplicating implementations

### Requirement: Infrastructure Layer Responsibility
The system SHALL confine persistence concerns, EF Core mappings, and repository implementations to the infrastructure layer.

#### Scenario: Infrastructure persistence ownership
- **GIVEN** workflow storage is configured
- **WHEN** database mappings are applied
- **THEN** persistence configuration is implemented in infrastructure and consumed via abstractions

### Requirement: Application Layer Orchestration
The system SHALL orchestrate use cases through application services, DTOs, and MediatR handlers without direct coupling to UI framework concerns.

#### Scenario: Application use case execution
- **GIVEN** a workflow command request is received from api
- **WHEN** the application handler processes the request
- **THEN** domain and persistence interactions occur through application-defined contracts

### Requirement: API Layer Surface
The system SHALL expose workflow-related behavior through Minimal API endpoints and delegate business logic to application services.

#### Scenario: API endpoint delegation
- **GIVEN** an HTTP request is received by api
- **WHEN** endpoint logic executes
- **THEN** orchestration is delegated to application services rather than embedded in endpoint code

### Requirement: UI Layer Interaction
The system SHALL provide a Blazor WebAssembly UI layer that composes editor components, calls api endpoints, and uses JS interop for canvas operations.

#### Scenario: UI to API interaction path
- **GIVEN** a user action in the editor
- **WHEN** the UI submits workflow operations
- **THEN** requests are sent to api endpoints and responses are rendered in UI state

### Requirement: Test Layer Coverage
The system SHALL maintain dedicated test projects for unit and integration verification of architecture boundaries and workflow behavior.

#### Scenario: Multi-layer validation tests
- **GIVEN** the solution test suite runs
- **WHEN** architecture and lifecycle tests execute
- **THEN** unit tests validate isolated components and integration tests validate end-to-end paths
