## 1. Database Connectivity Baseline

- [ ] 1.1 Configure PostgreSQL 18 connection settings in all projects that require database access using ConnectionString=Host=localhost;Port=5432;Database=GamificationFlow_DEV;Username=postgres;Password=postgres
- [ ] 1.2 Add configuration binding and dependency registration for the shared database connection
- [ ] 1.3 Establish initial persistence contract for the rules table with schema:
	  rules
	  (
		  "Id" uuid NOT NULL,
		  "Name" character varying(256) COLLATE pg_catalog."default" NOT NULL,
		  "Expression" character varying(1024) COLLATE pg_catalog."default" NOT NULL,
		  "RuleJson" text COLLATE pg_catalog."default" NOT NULL,
		  "Version" integer NOT NULL,
		  "IsActive" boolean NOT NULL,
		  "EffectiveFromUtc" timestamp with time zone,
		  "EffectiveToUtc" timestamp with time zone,
		  CONSTRAINT "PK_rules" PRIMARY KEY ("Id")
	  )

## 2. Solution Scaffold

- [ ] 2.1 dotnet new sln RulesEngineWorkflowEditor
- [ ] 2.2 dotnet new classlib -n RulesEngine.Core
- [ ] 2.3 dotnet new classlib -n RulesEngine.Editor.Shared
- [ ] 2.4 dotnet new classlib -n RulesEngine.Infrastructure
- [ ] 2.5 dotnet new classlib -n RulesEngine.Application
- [ ] 2.6 dotnet new webapi -n RulesEngine.API
- [ ] 2.7 dotnet new blazorwasm -n RulesEngine.UI
- [ ] 2.8 dotnet new xunit -n RulesEngine.Tests
- [ ] 2.9 Add all projects to the solution and verify reference graph direction

## 3. Architecture Wiring

- [ ] 3.1 Add project references to enforce Core <- Application <- API/UI flow with Infrastructure behind interfaces
- [ ] 3.2 Add shared-editor contracts and helper abstractions
- [ ] 3.3 Add dependency injection registration extension methods per layer
- [ ] 3.4 Add baseline DTOs, commands, and MediatR handler shells

## 4. Workflow Lifecycle API

- [ ] 4.1 Add Minimal API route group for /api/workflows
- [ ] 4.2 Implement GET list and GET by id endpoints
- [ ] 4.3 Implement POST create endpoint with validation response contract
- [ ] 4.4 Implement PUT update endpoint with schema validation gate
- [ ] 4.5 Implement DELETE endpoint with proper HTTP semantics

## 5. Validation and Execution

- [ ] 5.1 Implement schema version resolution and validation service
- [ ] 5.2 Implement RulesEngine workflow registration wrapper in core
- [ ] 5.3 Implement dry-run execution path without persistence side effects
- [ ] 5.4 Implement real execution path with execution state persistence
- [ ] 5.5 Add structured error model for validation and execution failures

## 6. Persistence and Integrations

- [ ] 6.1 Implement EF Core DbContext and WorkflowEntity with JSON definition mapping
- [ ] 6.2 Implement repository interfaces and infrastructure implementations
- [ ] 6.3 Implement LogicFlow interop wrapper module (initializeCanvas, getGraphData, loadGraphData)
- [ ] 6.4 Implement /editor route in Blazor UI and wire Validate action to api
- [ ] 6.5 Add Radzen components for editor controls and feedback surfaces

## 7. Quality and Governance

- [ ] 7.1 Add unit tests for validation, execution orchestration, and repository behavior
- [ ] 7.2 Add integration tests for workflow CRUD and execution endpoints
- [ ] 7.3 Add JS interop contract tests for workflow canvas wrapper
- [ ] 7.4 Run openspec validate --strict and resolve all issues
- [ ] 7.5 Prepare archive readiness checklist for initial-docs change
