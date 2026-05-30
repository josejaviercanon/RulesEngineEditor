# Governance and Sprint Playbook – RulesEngine Workflow Editor

## Purpose
Provide unified governance for UI and backend agents, ensuring compliance, observability, and human‑in‑the‑loop debugging.

## Agentic Development Principles
- **Spec‑Driven**: All agent outputs validated against OpenSpec/Spec‑Kit.
- **Human‑in‑the‑Loop**: Agents automate routine tasks; humans approve critical changes.
- **Observability**: DevUI + OpenTelemetry for tracing and cost metrics.
- **Auditability**: Store approvals and revisions in SQL/PostgreSQL.

## Sprint Playbook
1. **Planning**
   - Define workflow editor features in `Architecture.md`.
   - Assign agent roles per project (`UI.AgentRoles.md`, `BE.AgentRoles.md`).

2. **Execution**
   - Agents generate schema exports, API stubs, and tests.
   - Humans debug and validate outputs.

3. **Review**
   - Conduct sprint review with Governance.md as checklist.
   - Ensure compliance logs updated in `QA.md` and `DevOps.md`.

4. **Deployment**
   - CI/CD builds UI and backend separately.
   - Publish unified artifacts with validated workflows.

## Cross‑Project Integration
- **UI → BE**: JSON schema export validated by backend.
- **BE → UI**: Validation results surfaced in React editor.
- **Shared Libraries**: RulesEngineWrapper ensures schema compliance across layers.

## Compliance Checklist
- [ ] Schema validation passes for all workflows.
- [ ] Human approval logged for critical changes.
- [ ] Debug guides followed for UI and BE issues.
- [ ] Observability traces captured in DevUI.
