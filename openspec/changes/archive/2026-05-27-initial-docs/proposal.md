## Why

RulesEngineWorkflowEditor has a new multi-project solution scaffold and legacy implementation assets, but it does not yet have a durable architecture and integration contract in OpenSpec change form. Capturing this initial documentation change now establishes a single baseline for implementation planning, validation, and future deltas.

## What Changes

- Add PostgreSQL 18 database connectivity baseline for all projects requiring persistence, including a shared connection configuration and initial rules-table contract.
- Add a documentation-first OpenSpec change named initial-docs to define architecture and integration contracts.
- Add delta specs for architecture layers, workflow lifecycle behavior, and external or cross-layer integrations.
- Add a consolidated technical design document that replaces separate architecture and integration writeups.
- Add a bootstrap tasks checklist for solution setup and initial implementation sequencing.

## Scope

- Define the initial persistence connection contract using PostgreSQL 18 and the canonical rules table schema for rule storage.
- Define normative requirements for clean architecture boundaries across core, shared-editor, infrastructure, application, api, ui, and tests.
- Define normative requirements for workflow CRUD lifecycle behavior and endpoint semantics.
- Define normative requirements for integration contracts: RulesEngine, LogicFlow.js interop, Radzen UI, EF Core persistence, and Minimal API.
- Document durable design decisions for JSON storage strategy, schema versioning, and dry-run versus real execution paths.

## Out of Scope

- Implementing runtime endpoints, persistence repositories, UI components, or JS interop modules.
- Migrating legacy code into the new src projects.
- Performance tuning, load testing, and production deployment hardening.

## Risks

- Documentation may diverge from implementation if follow-on changes are not validated against these specs.
- Layer boundaries may be interpreted inconsistently without explicit acceptance tests.
- Brownfield assumptions from legacy behavior may conflict with target architecture decisions.

## Capabilities

### New Capabilities

- architecture: Clean Architecture layer responsibilities and boundaries for the 7-project solution.
- workflow-lifecycle: Workflow CRUD and execution lifecycle requirements from API through persistence.
- integrations: Integration contracts for RulesEngine, LogicFlow.js interop, Radzen, EF Core, and Minimal API.

### Modified Capabilities

- None.

## Impact

- Artifacts affected: openspec/changes/initial-docs/proposal.md, design.md, tasks.md, and specs under architecture, workflow-lifecycle, and integrations.
- Code impact: none in this change (documentation and planning only).
- Process impact: establishes baseline contracts for subsequent implementation changes and strict validation gates.
