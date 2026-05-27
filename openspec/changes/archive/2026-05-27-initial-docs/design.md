## Context

RulesEngineWorkflowEditor is evolving from a legacy editor implementation toward a clean multi-project .NET solution that separates core logic, application orchestration, infrastructure persistence, api surface, and UI delivery. The immediate need is a durable architecture and integration map that guides implementation without forcing code migration in this documentation change.

## Goals / Non-Goals

**Goals:**
- Define explicit layer responsibilities across core, shared-editor, infrastructure, application, api, ui, and tests.
- Define integration boundaries for RulesEngine, LogicFlow.js interop, Radzen UI composition, EF Core persistence, and Minimal API endpoints.
- Establish a PostgreSQL 18 persistence baseline, including shared connection configuration and a canonical rules table contract.
- Define schema versioning expectations for workflow validation.
- Define execution behavior differences between dry-run and real execution paths.

**Non-Goals:**
- Implement endpoint handlers, repository code, or UI components in this change.
- Finalize vendor-specific performance tuning or deployment topology.
- Migrate all legacy assets into new projects.

## Decisions

### Decision: Layered Clean Architecture with 7 projects
- Core owns domain entities, RulesEngine wrapper abstractions, and schema validation policies.
- Editor.Shared owns cross-cutting editor contracts and validation helpers reused by ui and application.
- Infrastructure owns EF Core DbContext, JSON mapping, and repository implementations.
- Application owns use-case orchestration, DTO mapping, and MediatR handlers.
- API owns Minimal API endpoint mapping and HTTP contract translation.
- UI owns Blazor WASM composition, Radzen controls, and JS interop invocation.
- Tests own unit and integration verification across layers.

Alternatives considered:
- Monolithic API plus UI project with inline persistence logic.
- Rejected because it weakens boundary enforcement and makes testing and migration harder.

### Decision: JS interop wrapper pattern for LogicFlow.js
- UI components SHALL call a dedicated WorkflowCanvas interop wrapper module.
- Wrapper methods include initializeCanvas, getGraphData, and loadGraphData.
- Component code SHALL not couple directly to raw global LogicFlow APIs.

Alternatives considered:
- Direct component-level JS calls in each page/component.
- Rejected due to duplicate interop glue and unstable contract surface.

### Decision: PostgreSQL 18 connection baseline and rules table contract
- Database-capable services SHALL use PostgreSQL 18 connection settings via shared configuration and dependency injection wiring.
- The canonical baseline connection string for development is Host=localhost;Port=5432;Database=GamificationFlow_DEV;Username=postgres;Password=postgres.
- Rule persistence SHALL target the rules table contract with columns Id, Name, Expression, RuleJson, Version, IsActive, EffectiveFromUtc, and EffectiveToUtc, with PK_rules on Id.
- RuleJson SHALL carry serialized rule definition payloads while metadata columns remain queryable.

Alternatives considered:
- Fully normalized relational graph for every node and edge.
- Rejected for high migration churn and reduced flexibility during editor evolution.

### Decision: Schema versioning for validation
- Validation requests SHALL include or default to a schema version identifier.
- Version-aware validators SHALL execute before RulesEngine registration or execution.

Alternatives considered:
- Single unversioned schema.
- Rejected because it prevents safe evolution of workflow shape over time.

### Decision: Separate dry-run and real execution paths
- dryRun=true executes rules and returns results without persistence side effects.
- dryRun=false executes rules and persists execution state and audit metadata.

Alternatives considered:
- Single execution mode with optional post-processing.
- Rejected due to ambiguous side effects and weaker testability for simulation workflows.

## Risks / Trade-offs

- Risk: Reliance on a single shared development connection string can hide environment-specific misconfiguration. -> Mitigation: keep environment overrides explicit and validate configuration at startup.
- Risk: RuleJson text payloads may reduce ad-hoc relational query ergonomics. -> Mitigation: keep stable metadata columns queryable and add projections for common read paths.
- Risk: Interop wrapper can drift from LogicFlow capabilities. -> Mitigation: version wrapper interface and add contract tests around wrapper methods.
- Risk: Schema version proliferation can increase maintenance load. -> Mitigation: define version lifecycle policy and deprecation windows.
- Risk: Divergence between dry-run and real execution code paths can create inconsistent outcomes. -> Mitigation: share evaluation core and isolate only persistence side effects.

## Migration Plan

1. Create and validate OpenSpec artifacts for proposal, specs, design, and tasks.
2. Implement scaffold tasks for solution and project structure.
3. Implement vertical slice for validation plus dry-run execution path first.
4. Add persistence and real execution state handling.
5. Add UI editor route with LogicFlow interop wrapper and API integration.
6. Backfill unit and integration tests, then enforce strict spec validation in CI.

Rollback strategy:
- Documentation-only change can be rolled back by reverting change artifacts or abandoning the change before archive.

## Open Questions

- Which schema registry location should store versioned workflow schemas for runtime validation?
- What minimum execution audit fields are required for real execution persistence?
- Should Radzen adoption be full-surface or limited to editor and workflow operations first?
