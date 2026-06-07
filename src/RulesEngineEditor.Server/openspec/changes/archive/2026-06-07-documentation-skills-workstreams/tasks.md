## 1. Qwen Agent Skill File Creation

- [x] 1.1 Create `.agents/skills/qwen-architect.md` with model `qwen-plus`, permissions `edit=allow`/`bash=deny`, mirroring `deepseek-architect.md` structure
- [x] 1.2 Create `.agents/skills/qwen-builder.md` with model `qwen-max`, permissions `edit=allow`/`bash=allow`, mirroring `deepseek-builder.md` structure
- [x] 1.3 Create `.agents/skills/qwen-reviewer.md` with model `qwen-max`, permissions `edit=deny`/`bash=deny`, mirroring `deepseek-reviewer.md` structure

## 2. Documentation Review & Update

- [x] 2.1 Review `docs/BE.Architecture.md` against current codebase (Program.cs, controllers, services, .csproj) and update any outdated details
- [x] 2.2 Review `docs/BE.DebugGuide.md` against current tooling and configuration; update debugging steps, tool references, and connection details
- [x] 2.3 Update `docs/BE.AgentRoles.md` to include Qwen agent role assignments and reference both `deepseek-*.md` and `qwen-*.md` skill files

## 3. Skills Alignment Audit

- [x] 3.1 Re-assess all installed opencode skill plugins (`dotnet`, `dotnet-aspnet`, `dotnet-test`) for relevance with the combined Deepseek + Qwen agent roster
- [x] 3.2 Update `docs/BE.SkillsAudit.md` to include dual-model alignment assessment, updated coverage gaps, and revised recommendations
