## Context

The project currently has Deepseek-specific agent skill files (`.agents/skills/deepseek-architect.md`, `.agents/skills/deepseek-builder.md`, `.agents/skills/deepseek-reviewer.md`) and documentation (`docs/BE.AgentRoles.md`, `docs/BE.Architecture.md`, `docs/BE.DebugGuide.md`, `docs/BE.SkillsAudit.md`). Qwen model support is needed to enable multi-model agent workflows.

The work is organized into three parallel workstreams:
1. **Documentation review & update** — audit and refresh existing docs
2. **Qwen agent skill file creation** — create 3 new skill files mirroring Deepseek conventions
3. **Skills alignment audit** — re-assess skills for dual-model alignment

## Goals / Non-Goals

**Goals:**
- Create Qwen-specific agent skill files following the established Deepseek conventions
- Review and update all documentation in `docs/` for accuracy and completeness
- Refresh skills alignment audit to cover both Deepseek and Qwen agents
- Enable three parallel workstreams that can execute independently

**Non-Goals:**
- No changes to production code, API endpoints, or database schema
- No changes to Deepseek agent skill files (they remain as-is)
- No installation of new opencode skill plugins (assessment only)
- No changes to `.opencode/` configuration beyond skill files

## Decisions

| Decision | Rationale | Alternatives Considered |
|----------|-----------|------------------------|
| **Qwen model assignment**: Architect → `qwen-plus`, Builder → `qwen-max`, Reviewer → `qwen-max` | `qwen-plus` is faster/cheaper for planning; `qwen-max` is top-tier for coding and review. Mirrors Deepseek assignment logic (flash vs pro). | Using `qwen-max` for all roles (rejected — wastes cost on planning tasks) |
| **Mirror Deepseek structure** for Qwen skill files | Ensures consistency, reduces cognitive load when switching between agents, and makes maintenance predictable | Creating a combined skill file (rejected — harder to maintain, violates single-responsibility per agent) |
| **Three parallel workstreams** | Documentation, skill creation, and audit are independent — no dependencies between them | Sequential execution (rejected — wastes time on non-dependent tasks) |
| **Qwen permissions match Deepseek role permissions** | Architect plans only (edit only, no bash), builder codes (edit+bash), reviewer audits (read-only). Consistent security model across model families | Different permissions for Qwen vs Deepseek (rejected — role, not model, determines permissions) |

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Qwen skill files become stale if Deepseek files evolve | Include cross-reference note in Qwen files pointing to Deepseek as source of truth for technology stack details |
| Documentation updates miss subtle inaccuracies | Each doc is reviewed against the actual codebase (Program.cs, controllers, services, .csproj) |
| Skills audit may identify gaps requiring new plugins | Gaps are documented with severity; resolution deferred to follow-up changes unless critical |
