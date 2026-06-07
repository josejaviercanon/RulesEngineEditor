# Proposal: Review & Update Agentic Development Documentation

## Change Name
`review-update-agentic-docs`

## Summary
Comprehensive review and update of all project documentation related to agentic development, creation of Deepseek-specific agent skill files, and alignment audit of current agent skills against the ASP.NET Core 10 MVC WebAPI technology stack.

---

## What

### 1. Documentation Review & Updates
Review all 8 documentation files across two doc directories and update them to accurately reflect the current technology stack (ASP.NET Core 10 MVC WebAPI, EF Core 10, PostgreSQL, OpenAPI/Scalar, xUnit).

**Files in scope:**

| File | Location | Current State |
|------|----------|---------------|
| `Architecture.md` | `docs/` | Good baseline; minor updates needed |
| `Governance.md` | `docs/` | Brief; needs expansion |
| `QA.md` | `docs/` | References xUnit/NUnit — needs xUnit-only alignment |
| `SprintPlaybook.md` | `docs/` | **EMPTY** — needs full creation |
| `TestingStrategy.md` | `docs/` | Comprehensive; minor consistency fixes |
| `BE.AgentRoles.md` | `src/.../docs/` | Too generic; needs Deepseek agent specifics |
| `BE.Architecture.md` | `src/.../docs/` | References outdated "Playwright + Scalar" pattern |
| `BE.DebugGuide.md` | `src/.../docs/` | Basic; needs expansion for ASP.NET Core 10 |

### 2. Deepseek Agent Skill Files
Create project-specific `.md` agent files in `.agents/skills/` to provide Deepseek models (v4-flash for Architect, v4-pro for Builder/Reviewer) with project context, technology constraints, and coding standards.

### 3. Skills Alignment Audit
Audit the 3 installed skill plugins (dotnet, dotnet-aspnet, dotnet-test) against the actual ASP.NET Core 10 MVC WebAPI stack to identify:
- Skills that are relevant and properly aligned
- Skills that are irrelevant (e.g., Blazor Server conversion)
- Missing skill coverage (e.g., EF Core, OpenAPI/Scalar)

---

## Why

1. **Documentation is fragmented and inconsistent**: The `SprintPlaybook.md` is empty, `BE.Architecture.md` references patterns dropped in `TestingStrategy.md`, and `QA.md` mentions NUnit despite the xUnit-only decision.

2. **No Deepseek-specific agent context**: The `opencode.json` defines three agents using Deepseek models (architect → v4-flash, builder → v4-pro, reviewer → v4-pro), but the `.agents/skills/` directory is empty. Without project-specific agent skill files, Deepseek models lack critical context about the ASP.NET Core 10 stack, coding conventions, and project constraints.

3. **Skills alignment is unverified**: 42+ skills are installed across 3 plugins, but no audit has confirmed they match the actual technology choices. The `convert-blazor-server-to-webapp` skill is irrelevant for an MVC WebAPI project, while EF Core and OpenAPI/Scalar patterns lack dedicated skill coverage.

4. **Agentic development governance requires documentation**: The project follows Spec-Driven Development with human-in-the-loop principles. Agents cannot effectively plan, build, or review without accurate, current documentation.

---

## Scope

### In Scope
- Update all 8 documentation files
- Create `SprintPlaybook.md` from scratch
- Create Deepseek agent `.md` files for all 3 agent roles
- Skills alignment audit report with recommendations
- Fix cross-document inconsistencies

### Out of Scope
- Implementation of any backend features (Controllers, Services, etc.)
- Changes to `opencode.json` agent model assignments
- Installation of new skill plugins (recommendations only)
- Frontend (React) documentation updates
- CI/CD pipeline creation (`.github/workflows/`)

---

## Success Criteria
- [ ] All 8 documentation files reviewed and updated
- [ ] `SprintPlaybook.md` populated with complete sprint workflow
- [ ] 3 Deepseek agent skill files created in `.agents/skills/`
- [ ] Skills alignment audit documented with actionable recommendations
- [ ] No cross-document inconsistencies remain
- [ ] All docs reference ASP.NET Core 10 (not generic ".NET Core")
- [ ] All docs reference xUnit only (no NUnit/MSTest mentions)
