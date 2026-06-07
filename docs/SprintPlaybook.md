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
