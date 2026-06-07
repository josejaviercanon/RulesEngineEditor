## Purpose

Define requirements for auditing installed opencode skill plugins for alignment with the combined Deepseek and Qwen agent roster, and updating the skills audit documentation accordingly.

## Requirements

### Requirement: Skills audit refreshed to include Qwen agent alignment
The `docs/BE.SkillsAudit.md` SHALL be updated to assess all installed opencode skill plugins for alignment with both Deepseek and Qwen agent roles.

#### Scenario: Skills audit covers dual-model alignment
- **WHEN** reviewing `docs/BE.SkillsAudit.md`
- **THEN** it SHALL include an assessment of skill relevance for both Deepseek and Qwen agents, noting any gaps or overlaps

#### Scenario: Audit recommendations updated for Qwen
- **WHEN** reading the Recommendations section
- **THEN** it SHALL address whether new skill plugins are needed for Qwen-specific patterns or whether existing Deepseek recommendations remain sufficient

### Requirement: Installed opencode skills re-assessed for continued relevance
All installed opencode skills SHALL be re-assessed for relevance with the expanded Deepseek + Qwen agent roster.

#### Scenario: Each skill plugin has a relevance rating
- **WHEN** inspecting `docs/BE.SkillsAudit.md`
- **THEN** every installed skill plugin (`dotnet`, `dotnet-aspnet`, `dotnet-test` with their sub-skills) SHALL have a relevance rating (Highly relevant / Relevant / Low relevance / Not relevant) and a brief assessment

#### Scenario: Coverage gaps section updated
- **WHEN** reading the Coverage Gaps Identified section
- **THEN** it SHALL reflect whether gaps are addressed by Qwen agent skill files, Deepseek agent skill files, or require new plugins
