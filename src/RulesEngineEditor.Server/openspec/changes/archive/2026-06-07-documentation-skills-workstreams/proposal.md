## Why

The project currently has Deepseek-specific agent skill files but needs Qwen model support to enable multi-model agent workflows. Existing documentation (`BE.AgentRoles.md`, `BE.Architecture.md`, `BE.DebugGuide.md`, `BE.SkillsAudit.md`) requires review and updates to reflect current project state and emerging patterns. The skills alignment audit must be refreshed to include Qwen capabilities alongside existing Deepseek coverage.

## What Changes

- Create Qwen agent skill files (`.agents/skills/qwen-*.md`) for architect, builder, and reviewer roles
- Review and update existing documentation in `docs/` to reflect current architecture, agent roles, and debugging practices
- Update `BE.SkillsAudit.md` to include Qwen skills alignment assessment
- Audit all installed opencode skill plugins for continued relevance with the expanded agent roster

## Capabilities

### New Capabilities
- `qwen-agent-skills`: Qwen model agent skill files covering architect, builder, and reviewer roles with Qwen-specific model assignments, conventions, and permissions
- `documentation-review`: Review and update existing project documentation (`BE.AgentRoles.md`, `BE.Architecture.md`, `BE.DebugGuide.md`, `BE.SkillsAudit.md`) for accuracy, completeness, and cross-referencing
- `skills-alignment-audit`: Audit all installed opencode skill plugins against the combined Deepseek + Qwen agent roster, identifying gaps, redundancies, and recommendations

### Modified Capabilities
*None — no existing specs to modify.*

## Impact

- `.agents/skills/` directory gains up to 3 new files (qwen-architect.md, qwen-builder.md, qwen-reviewer.md)
- `docs/BE.AgentRoles.md` updated to include Qwen agent role assignments
- `docs/BE.SkillsAudit.md` refreshed with dual-model alignment assessment
- `docs/BE.Architecture.md` and `docs/BE.DebugGuide.md` reviewed and updated as needed
- No production code, API endpoints, or database schema changes
