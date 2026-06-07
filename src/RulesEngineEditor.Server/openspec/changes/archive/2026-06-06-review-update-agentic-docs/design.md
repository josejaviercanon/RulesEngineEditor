# Design: Review & Update Agentic Development Documentation

## Overview

This design document details the approach for reviewing and updating all project documentation, creating Deepseek agent skill files, and auditing skills alignment. The work is organized into three workstreams that can be executed in parallel.

---

## Workstream 1: Documentation Review & Updates

### 1.1 Findings Matrix

| Document | Issue | Severity | Action |
|----------|-------|----------|--------|
| `docs/SprintPlaybook.md` | Empty file (0 lines) | Critical | Create from scratch |
| `docs/QA.md` | References "xUnit/NUnit" — project decided xUnit-only | High | Remove NUnit references |
| `src/.../BE.Architecture.md` | References "Playwright + Scalar" E2E pattern dropped in TestingStrategy.md | High | Remove outdated E2E section, align with TestingStrategy |
| `src/.../BE.AgentRoles.md` | Generic agent roles; no Deepseek model context | Medium | Add Deepseek agent specifics and model assignments |
| `src/.../BE.DebugGuide.md` | Basic; missing ASP.NET Core 10 specifics | Medium | Expand with hot reload, DevUI, OpenTelemetry |
| `docs/Architecture.md` | Good baseline; minor version references | Low | Verify ASP.NET Core 10 references, update solution structure |
| `docs/Governance.md` | Brief; lacks sprint ceremony details | Low | Expand with review checklists |
| `docs/TestingStrategy.md` | Comprehensive; minor consistency fix needed | Low | Remove residual Playwright+Scalar mention |

### 1.2 SprintPlaybook.md — Full Creation

The empty `docs/SprintPlaybook.md` needs to be created with the following structure:

```
# Sprint Playbook – RulesEngine Workflow Editor

## Sprint Cadence
- Sprint duration: 2 weeks
- Planning: Day 1 (Monday)
- Review/Demo: Last day (Friday of week 2)
- Retrospective: Same day as Review

## Phase 1: Planning
- Define features in Architecture.md
- Assign agent roles per BE.AgentRoles.md
- Create OpenSpec change proposals for each feature
- Prioritize backlog items in QA.md checklist

## Phase 2: Execution
- Architect agent creates specs via /opsx-propose
- Builder agent implements via /opsx-apply
- Reviewer agent verifies via /opsx-verify
- Human reviews all agent-generated PRs

## Phase 3: Review
- Sprint review with Governance.md as checklist
- QA.md compliance verification
- Test coverage review (target: 80% unit, key integration paths)
- Documentation accuracy check

## Phase 4: Deployment
- CI/CD builds UI and backend separately
- Schema validation gates the pipeline
- Human approval required for production deploy

## Agent Workflow Integration
- All features start with OpenSpec proposal
- Specs reviewed before implementation begins
- Builder follows tasks.md strictly
- Reviewer audits implementation against specs
- Archive completed changes with /opsx-archive

## Human-in-the-Loop Checkpoints
- PR approval required for all agent-generated code
- EF Core migrations reviewed manually
- API contract changes validated with Postman/Scalar
- Architecture changes require team consensus
```

### 1.3 QA.md — xUnit Alignment

Remove the NUnit reference on line 9:
- **Before**: `Unit tests pass in BE.Tests (xUnit/NUnit).`
- **After**: `Unit tests pass in BE.Tests (xUnit).`

### 1.4 BE.Architecture.md — Remove Outdated E2E Pattern

Replace lines 82-84 (the "Optional E2E (Playwright + Scalar)" section) with a note aligning with TestingStrategy.md:
- Remove the Playwright + Scalar recommendation
- Add reference to Playwright's native `APIRequestContext` for headless API testing if needed

### 1.5 BE.AgentRoles.md — Deepseek Agent Context

Expand to include:
- Deepseek model assignments from `opencode.json`
- Agent-specific technology constraints
- Permission boundaries (edit/bash access)
- Cross-reference to `.agents/skills/` for agent skill files

### 1.6 BE.DebugGuide.md — ASP.NET Core 10 Expansion

Add sections for:
- .NET Hot Reload (`dotnet watch`)
- OpenAPI/Scalar UI debugging
- OpenTelemetry trace inspection
- EF Core migration debugging
- PostgreSQL connection troubleshooting

---

## Workstream 2: Deepseek Agent Skill Files

### 2.1 Design Decision: File Structure

Create three agent skill files in `.agents/skills/`, one per agent role defined in `opencode.json`:

```
.agents/skills/
├── deepseek-architect.md      # Architect agent (v4-flash)
├── deepseek-builder.md        # Builder agent (v4-pro)
└── deepseek-reviewer.md       # Reviewer agent (v4-pro)
```

### 2.2 deepseek-architect.md

**Purpose**: Provide the Architect agent (deepseek-v4-flash) with project context for planning.

**Content sections**:
- Project technology stack summary (ASP.NET Core 10, EF Core 10, PostgreSQL, xUnit)
- OpenSpec workflow expectations
- Documentation structure and locations
- Constraints: planning-only, no implementation code
- Key architectural decisions to enforce (xUnit-only, no EF Core In-Memory, MVC controllers over minimal APIs)

### 2.3 deepseek-builder.md

**Purpose**: Provide the Builder agent (deepseek-v4-pro) with coding standards and technology constraints.

**Content sections**:
- ASP.NET Core 10 MVC WebAPI patterns to follow
- Controller conventions (attribute routing, OpenAPI metadata, error handling)
- EF Core 10 patterns (repository pattern, migrations, PostgreSQL-specific features)
- Coding standards (nullable enabled, implicit usings, async/await patterns)
- Testing requirements (xUnit, Testcontainers for integration, no In-Memory provider)
- Package versions and constraints from `.csproj`

### 2.4 deepseek-reviewer.md

**Purpose**: Provide the Reviewer agent (deepseek-v4-pro) with review criteria and quality gates.

**Content sections**:
- Review checklist aligned with Governance.md
- ASP.NET Core 10 best practices to verify
- Security review points (CORS, HTTPS, authentication)
- Testing adequacy criteria (xUnit patterns, Testcontainers usage)
- Documentation accuracy verification
- OpenSpec compliance checks

---

## Workstream 3: Skills Alignment Audit

### 3.1 Audit Methodology

For each installed skill, evaluate:
1. **Relevance**: Does the skill apply to ASP.NET Core 10 MVC WebAPI?
2. **Accuracy**: Are the skill's patterns correct for .NET 10?
3. **Coverage Gap**: Is there a needed capability without a matching skill?

### 3.2 Audit Results

#### dotnet plugin (3 skills)

| Skill | Relevance | Notes |
|-------|-----------|-------|
| `csharp-scripts` | Relevant | Useful for quick C# experiments and prototyping |
| `dotnet-pinvoke` | Low relevance | P/Invoke unlikely needed for a Web API project |
| `nuget-trusted-publishing` | Relevant | Useful if publishing NuGet packages via CI/CD |

#### dotnet-aspnet plugin (4 skills)

| Skill | Relevance | Notes |
|-------|-----------|-------|
| `dotnet-webapi` | **Highly relevant** | Core skill for MVC WebAPI endpoint creation |
| `configuring-opentelemetry-dotnet` | **Highly relevant** | Aligns with observability governance requirement |
| `minimal-api-file-upload` | Relevant | Useful if file upload endpoints are added |
| `convert-blazor-server-to-webapp` | **Not relevant** | Project is MVC WebAPI, not Blazor — can be ignored |

#### dotnet-test plugin (25 skills + 11 agents)

| Category | Relevance | Notes |
|----------|-----------|-------|
| Test execution (run-tests, mtp-hot-reload) | **Highly relevant** | Core test running capabilities |
| Test generation (code-testing-agent, writing-mstest-tests) | Partially relevant | code-testing-agent is polyglot and useful; writing-mstest-tests not needed (xUnit project) |
| Test migration (migrate-*) | Low relevance | No test migration needed for new project |
| Test quality (test-anti-patterns, etc.) | **Highly relevant** | All 6 polyglot analysis skills applicable |
| Coverage & risk (coverage-analysis, crap-score) | **Highly relevant** | .NET coverage tools needed |
| Testability (detect-static, generate-wrapper, migrate-static) | **Highly relevant** | C# testability improvement tools |
| Reference data (platform-detection, filter-syntax, etc.) | **Highly relevant** | Supporting data for test operations |

### 3.3 Coverage Gaps Identified

| Gap | Impact | Recommendation |
|-----|--------|----------------|
| No EF Core-specific skill | Medium | Document EF Core 10 patterns in deepseek-builder.md agent skill file |
| No OpenAPI/Scalar skill | Low | dotnet-webapi skill covers OpenAPI metadata; Scalar is configured in Program.cs |
| No PostgreSQL/Npgsql skill | Low | Document Npgsql patterns in deepseek-builder.md |
| No CORS configuration skill | Low | CORS is already configured in Program.cs; document in debug guide |

### 3.4 Recommendation Summary

- **Keep all installed skills** — even low-relevance ones don't harm and may be useful edge cases
- **No new skill plugins needed** — gaps are better addressed through Deepseek agent skill files
- **Ignore `convert-blazor-server-to-webapp`** — not applicable but harmless
- **Focus agent skill files** on EF Core 10, PostgreSQL, and OpenAPI patterns to fill coverage gaps

---

## Cross-Cutting Concerns

### Document Naming Convention
- Global docs: `docs/<Topic>.md` (PascalCase)
- Backend docs: `src/.../docs/BE.<Topic>.md` (PascalCase with BE. prefix)
- Agent skills: `.agents/skills/<agent-name>.md` (kebab-case)

### Version References
All documents must reference:
- **ASP.NET Core 10** (not ".NET Core" or ".NET 8/9")
- **EF Core 10** (not generic "EF Core")
- **xUnit** (not "xUnit/NUnit" or "xUnit/MSTest")
- **Visual Studio 2026** (where IDE is referenced)
- **.NET 10 SDK** (for CLI references)

### Consistency Rules
- Testing strategy: xUnit for backend, Vitest+RTL+Playwright for frontend
- Database: PostgreSQL via Npgsql EF Core provider (not SQL Server for primary)
- API docs: OpenAPI + Scalar (not Swagger UI)
- E2E API testing: Playwright `APIRequestContext` (not Playwright + Scalar)
- Test isolation: Testcontainers for .NET (not EF Core In-Memory)
