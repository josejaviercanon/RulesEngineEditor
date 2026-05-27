## 1. Database Connectivity Baseline

- [x] 1.1 Configure PostgreSQL 18 connection settings in all projects that require database access using ConnectionString=Host=localhost;Port=5432;Database=GamificationFlow_DEV;Username=postgres;Password=postgres
- [x] 1.2 Add configuration binding and dependency registration for the shared database connection
- [x] 1.3 Establish initial persistence contract for the rules table with schema:
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

- [x] 2.1 dotnet new sln RulesEngineWorkflowEditor
- [x] 2.2 dotnet new classlib -n RulesEngine.Core
- [x] 2.3 dotnet new classlib -n RulesEngine.Editor.Shared
- [x] 2.4 dotnet new classlib -n RulesEngine.Infrastructure
- [x] 2.5 dotnet new classlib -n RulesEngine.Application
- [x] 2.6 dotnet new webapi -n RulesEngine.API
- [x] 2.7 dotnet new blazorwasm -n RulesEngine.UI
- [x] 2.8 dotnet new xunit -n RulesEngine.Tests
- [x] 2.9 Add all projects to the solution and verify reference graph direction

## 3. Architecture Wiring

- [x] 3.1 Add project references to enforce Core <- Application <- API/UI flow with Infrastructure behind interfaces
- [x] 3.2 Add shared-editor contracts and helper abstractions
- [x] 3.3 Add dependency injection registration extension methods per layer
- [x] 3.4 Add baseline DTOs, commands, and MediatR handler shells

## 4. Workflow Lifecycle API

- [x] 4.1 Add Minimal API route group for /api/workflows
- [x] 4.2 Implement GET list and GET by id endpoints
- [x] 4.3 Implement POST create endpoint with validation response contract
- [x] 4.4 Implement PUT update endpoint with schema validation gate
- [x] 4.5 Implement DELETE endpoint with proper HTTP semantics

## 5. Validation and Execution

- [x] 5.1 Implement schema version resolution and validation service
- [x] 5.2 Implement RulesEngine workflow registration wrapper in core
- [x] 5.3 Implement dry-run execution path without persistence side effects
- [x] 5.4 Implement real execution path with execution state persistence
- [x] 5.5 Add structured error model for validation and execution failures

## 6. Persistence and Integrations

- [x] 6.1 Implement EF Core DbContext and WorkflowEntity with JSON definition mapping
- [x] 6.2 Implement repository interfaces and infrastructure implementations
- [x] 6.3 Implement LogicFlow interop wrapper module (initializeCanvas, getGraphData, loadGraphData)
- [x] 6.4 Implement /editor route in Blazor UI and wire Validate action to api
- [x] 6.5 Add Radzen components for editor controls and feedback surfaces

## 7. Quality and Governance

- [x] 7.1 Add unit tests for validation, execution orchestration, and repository behavior
- [x] 7.2 Add integration tests for workflow CRUD and execution endpoints
- [x] 7.3 Add JS interop contract tests for workflow canvas wrapper
- [x] 7.4 Run openspec validate --strict and resolve all issues
- [x] 7.5 Prepare archive readiness checklist for initial-docs change
