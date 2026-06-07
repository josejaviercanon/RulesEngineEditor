## ADDED Requirements

### Requirement: Qwen agent skill files exist for architect, builder, and reviewer roles
The system SHALL have Qwen-specific agent skill files at `.agents/skills/qwen-architect.md`, `.agents/skills/qwen-builder.md`, and `.agents/skills/qwen-reviewer.md`.

#### Scenario: All three Qwen skill files are created
- **WHEN** the change is implemented
- **THEN** files `qwen-architect.md`, `qwen-builder.md`, and `qwen-reviewer.md` SHALL exist in `.agents/skills/`

#### Scenario: Each skill file has correct model assignment
- **WHEN** inspecting any Qwen skill file
- **THEN** it SHALL specify the correct Qwen model (`qwen-max` or `qwen-plus`) based on the agent role (architect=fast/efficient, builder=top-tier coding, reviewer=high-reasoning)

### Requirement: Qwen architect skill follows deepseek-architect conventions
The `qwen-architect.md` SHALL mirror the structure and sections of `deepseek-architect.md` but with Qwen-specific model assignments, technology stack references, and role permissions.

#### Scenario: Architect skill file structure matches convention
- **WHEN** comparing `qwen-architect.md` to `deepseek-architect.md`
- **THEN** the Qwen version SHALL contain the same sections (Model, Purpose, Technology Stack, OpenSpec Workflow, Documentation Structure, Key Architectural Decisions)

#### Scenario: Qwen architect uses correct model and permissions
- **WHEN** reading the Qwen architect skill
- **THEN** it SHALL specify `qwen-plus` as the model and `edit=allow`, `bash=deny` permissions

### Requirement: Qwen builder skill follows deepseek-builder conventions
The `qwen-builder.md` SHALL mirror the structure and sections of `deepseek-builder.md` but with Qwen-specific model assignments.

#### Scenario: Builder skill file structure matches convention
- **WHEN** comparing `qwen-builder.md` to `deepseek-builder.md`
- **THEN** the Qwen version SHALL contain the same sections (Model, Purpose, Technology Stack, ASP.NET Core 10 MVC Patterns, EF Core 10 Patterns, Coding Standards, Testing Requirements, Package Version Reference)

#### Scenario: Qwen builder uses correct model and permissions
- **WHEN** reading the Qwen builder skill
- **THEN** it SHALL specify `qwen-max` as the model and `edit=allow`, `bash=allow` permissions

### Requirement: Qwen reviewer skill follows deepseek-reviewer conventions
The `qwen-reviewer.md` SHALL mirror the structure and sections of `deepseek-reviewer.md` but with Qwen-specific model assignments.

#### Scenario: Reviewer skill file structure matches convention
- **WHEN** comparing `qwen-reviewer.md` to `deepseek-reviewer.md`
- **THEN** the Qwen version SHALL contain the same checklist sections (Proposal Review, Design Review, Task Review, Code Review, Security Review Points, Testing Review, Documentation Accuracy, OpenSpec Compliance)

#### Scenario: Qwen reviewer uses correct model and permissions
- **WHEN** reading the Qwen reviewer skill
- **THEN** it SHALL specify `qwen-max` as the model and `edit=deny`, `bash=deny` permissions
