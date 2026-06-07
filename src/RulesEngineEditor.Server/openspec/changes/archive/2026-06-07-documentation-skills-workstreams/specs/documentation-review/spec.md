## ADDED Requirements

### Requirement: BE.AgentRoles.md updated with Qwen agent roles
The `docs/BE.AgentRoles.md` SHALL be updated to include Qwen agent role assignments alongside existing Deepseek assignments.

#### Scenario: AgentRoles includes Qwen model assignments
- **WHEN** reviewing `docs/BE.AgentRoles.md`
- **THEN** it SHALL list Qwen models (`qwen-plus` for architect, `qwen-max` for builder/reviewer) with their role descriptions and permissions

#### Scenario: Agent skill files section references both Deepseek and Qwen
- **WHEN** reading the Agent Skill Files section
- **THEN** it SHALL reference both `deepseek-*.md` and `qwen-*.md` files in `.agents/skills/`

### Requirement: BE.Architecture.md reviewed and updated if needed
The `docs/BE.Architecture.md` SHALL be reviewed for accuracy against the current project state and updated if any structural or architectural details are outdated.

#### Scenario: Architecture doc reflects current project structure
- **WHEN** inspecting `docs/BE.Architecture.md`
- **THEN** its project structure, component descriptions, API endpoints, integration points, and testing strategy SHALL match the current codebase

### Requirement: BE.DebugGuide.md reviewed and updated if needed
The `docs/BE.DebugGuide.md` SHALL be reviewed for accuracy and updated if any debugging steps, tool references, or configuration details are outdated.

#### Scenario: Debug guide reflects current tooling
- **WHEN** inspecting `docs/BE.DebugGuide.md`
- **THEN** its debugging steps, tool references (Scalar, OpenTelemetry, PostgreSQL), and connection details SHALL match the current project configuration
