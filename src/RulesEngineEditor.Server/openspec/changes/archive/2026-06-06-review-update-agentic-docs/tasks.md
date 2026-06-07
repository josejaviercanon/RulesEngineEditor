# Tasks: Review & Update Agentic Development Documentation

## Workstream 1: Documentation Updates

- [x] **Task 1.1**: Create `docs/SprintPlaybook.md` with full sprint workflow content
  - File: `docs/SprintPlaybook.md` (currently empty)
  - Content: Sprint cadence, 4 phases (Planning, Execution, Review, Deployment), agent workflow integration, human-in-the-loop checkpoints
  - Reference: `design.md` §1.2 for structure
  - Verify: File has >50 lines of meaningful content

- [x] **Task 1.2**: Update `docs/QA.md` — remove NUnit reference, align with xUnit-only decision
  - File: `docs/QA.md`
  - Change line 9: `xUnit/NUnit` → `xUnit`
  - Verify: No NUnit or MSTest references remain in the file

- [x] **Task 1.3**: Update `src/.../docs/BE.Architecture.md` — remove outdated Playwright + Scalar E2E section
  - File: `src/RulesEngineEditor.Server/docs/BE.Architecture.md`
  - Replace lines 82-84 (Optional E2E section) with TestingStrategy-aligned guidance
  - Add reference to Playwright `APIRequestContext` for headless API testing
  - Verify: No "Playwright + Scalar" pattern mentioned

- [x] **Task 1.4**: Update `src/.../docs/BE.AgentRoles.md` — add Deepseek agent model context
  - File: `src/RulesEngineEditor.Server/docs/BE.AgentRoles.md`
  - Add section: Agent model assignments (Architect → v4-flash, Builder → v4-pro, Reviewer → v4-pro)
  - Add section: Permission boundaries (edit/bash) per agent
  - Add section: Cross-reference to `.agents/skills/` for detailed agent skill files
  - Verify: All 3 agent roles documented with model and permission info

- [x] **Task 1.5**: Update `src/.../docs/BE.DebugGuide.md` — expand for ASP.NET Core 10
  - File: `src/RulesEngineEditor.Server/docs/BE.DebugGuide.md`
  - Add section: .NET Hot Reload (`dotnet watch`)
  - Add section: OpenAPI/Scalar UI debugging at `/scalar/v1`
  - Add section: OpenTelemetry trace inspection
  - Add section: PostgreSQL connection troubleshooting (Npgsql)
  - Add section: EF Core 10 migration debugging
  - Verify: Debug guide covers all major technology components

- [x] **Task 1.6**: Update `docs/Architecture.md` — verify version references and solution structure
  - File: `docs/Architecture.md`
  - Verify all references say "ASP.NET Core 10" (not generic versions)
  - Update solution structure to match actual layout (`RulesEngineEditor.Server`, `rulesengineeditor.client`, `BE.Libraries`, `BE.Tests`, `UI.Tests`)
  - Verify: Solution structure matches actual `src/` directory

- [x] **Task 1.7**: Update `docs/Governance.md` — expand with review checklists
  - File: `docs/Governance.md`
  - Add section: Agent output review checklist (spec accuracy, code quality, test coverage)
  - Add section: Cross-project dependency checklist (UI ↔ BE integration points)
  - Verify: Governance doc has actionable review checklists

- [x] **Task 1.8**: Update `docs/TestingStrategy.md` — fix residual inconsistency
  - File: `docs/TestingStrategy.md`
  - Verify line 37 note about dropping Playwright + Scalar is clear and prominent
  - Ensure no contradictory E2E patterns remain
  - Verify: Single, consistent E2E testing approach documented

## Workstream 2: Deepseek Agent Skill Files

- [x] **Task 2.1**: Create `.agents/skills/deepseek-architect.md`
  - File: `src/RulesEngineEditor.Server/.agents/skills/deepseek-architect.md`
  - Sections: Technology stack summary, OpenSpec workflow, doc structure, planning-only constraints, key architectural decisions
  - Must reference: ASP.NET Core 10, EF Core 10, PostgreSQL, xUnit, MVC controllers
  - Verify: File exists and provides sufficient context for planning agent

- [x] **Task 2.2**: Create `.agents/skills/deepseek-builder.md`
  - File: `src/RulesEngineEditor.Server/.agents/skills/deepseek-builder.md`
  - Sections: ASP.NET Core 10 MVC patterns, controller conventions, EF Core 10 patterns, coding standards, testing requirements, package versions
  - Must include: nullable enabled, implicit usings, async/await patterns, attribute routing, OpenAPI metadata
  - Must include: Testcontainers for integration tests, no EF Core In-Memory
  - Verify: File exists and provides sufficient context for implementation agent

- [x] **Task 2.3**: Create `.agents/skills/deepseek-reviewer.md`
  - File: `src/RulesEngineEditor.Server/.agents/skills/deepseek-reviewer.md`
  - Sections: Review checklist, ASP.NET Core 10 best practices, security review points, testing adequacy criteria, documentation accuracy, OpenSpec compliance
  - Must include: CORS verification, HTTPS enforcement, xUnit pattern verification, Testcontainers usage check
  - Verify: File exists and provides sufficient context for review agent

## Workstream 3: Skills Alignment Audit

- [x] **Task 3.1**: Document skills alignment audit results
  - File: `src/RulesEngineEditor.Server/docs/BE.SkillsAudit.md` (new file)
  - Content: Full audit table from `design.md` §3.2
  - Include: Relevance ratings, coverage gaps, recommendations
  - Include: Action items for each gap (addressed via agent skill files vs. new plugin needed)
  - Verify: Audit document covers all 3 installed plugins (32+ skills)

## Cross-Cutting Verification

- [x] **Task 4.1**: Cross-document consistency check (clean — no NUnit/MSTest, no Playwright+Scalar, all refs consistent)
  - Verify: No document references "NUnit" or "MSTest" for backend testing
  - Verify: No document references "Playwright + Scalar" E2E pattern
  - Verify: All documents reference "ASP.NET Core 10" consistently
  - Verify: All documents reference "EF Core 10" consistently
  - Verify: All documents reference "xUnit" as sole backend test framework
  - Verify: All documents reference "PostgreSQL" as primary database
  - Verify: All documents reference "Scalar" (not "Swagger UI") for API docs

- [x] **Task 4.2**: Verify all new/updated files exist and are non-empty (all 5 new files confirmed, all 8 updated docs verified)
  - Check: `docs/SprintPlaybook.md` has content
  - Check: `.agents/skills/deepseek-architect.md` exists
  - Check: `.agents/skills/deepseek-builder.md` exists
  - Check: `.agents/skills/deepseek-reviewer.md` exists
  - Check: `docs/BE.SkillsAudit.md` exists
  - Check: All 8 original docs updated where needed
