## Context

RulesEngine workflows are authored as JSON documents where each workflow contains an ordered list of rules with expression strings. The current backend model focuses on decomposed fields and does not guarantee exact JSON fidelity for full workflow payloads or per-rule expression serialization. This introduces drift risk between authored workflow definitions and persisted data used for retrieval, versioning, and execution.

The change spans API contracts, application mappings, domain entities, and database schema. It must preserve backward compatibility for existing data while introducing canonical JSON persistence fields.

## Goals / Non-Goals

**Goals:**
- Persist full workflow JSON in `Workflow.WorkflowJson` as canonical source data for the complete workflow payload, including all rules.
- Persist per-rule JSON in `Rule.RuleJson`, including the plain-text `Expression` representation.
- Keep DTO/entity mapping consistent so JSON fields round-trip across create, update, and read operations.
- Add database migration changes with safe defaults for existing records.

**Non-Goals:**
- Redesigning RulesEngine expression syntax or evaluation semantics.
- Replacing existing structured workflow/rule columns that remain required for queries and lifecycle logic.
- Implementing large-scale historical data rehydration beyond migration-safe initialization/backfill.

## Decisions

1. Canonical workflow payload storage on workflow rows
- Decision: Add `WorkflowJson` to the workflow entity/table and persist the full workflow document exactly as serialized JSON.
- Rationale: Guarantees high-fidelity archival and retrieval aligned with RulesEngine documented format.
- Alternative considered: Reconstruct workflow JSON from relational rule columns on demand.
- Why not chosen: Reconstruction can lose original shape/ordering and increases runtime complexity.

2. Canonical per-rule payload storage on rule rows
- Decision: Add/standardize `RuleJson` on the rule entity/table and ensure it includes `Expression` as plain text within JSON.
- Rationale: Keeps each rule revision self-describing and stable for diagnostics/audit/version restore.
- Alternative considered: Store only `Expression` text in a dedicated column.
- Why not chosen: Loses surrounding rule structure and creates split serialization responsibilities.

3. Dual representation policy (structured + canonical JSON)
- Decision: Maintain both structured fields and JSON fields, with deterministic mapping rules during writes.
- Rationale: Structured fields support existing lifecycle and query behavior; JSON fields preserve exact payload semantics.
- Alternative considered: Move to JSON-only persistence.
- Why not chosen: Would break existing queries/logic and require broad refactoring.

4. Migration strategy for existing rows
- Decision: Add nullable columns initially (or default empty JSON where appropriate), then backfill from current fields where feasible.
- Rationale: Minimizes deployment risk and supports incremental rollout.
- Alternative considered: Non-null columns with immediate full backfill requirement.
- Why not chosen: High risk for large datasets and partial data quality edge cases.

## Risks / Trade-offs

- [JSON/structured divergence on updates] -> Mitigation: enforce write-path mapping that regenerates JSON from inbound canonical DTO inputs and add consistency tests.
- [Migration complexity for legacy rows] -> Mitigation: phased migration with nullable introduction, idempotent backfill job, and fallback handling when JSON is absent.
- [Payload size growth in DB] -> Mitigation: monitor storage/index impact, keep JSON columns non-indexed unless query needs emerge.
- [Backward compatibility for existing API clients] -> Mitigation: preserve current contracts while adding non-breaking JSON fields in DTOs where needed.

## Migration Plan

1. Add DB migration for `WorkflowJson` and `RuleJson` columns with safe nullability/default handling.
2. Deploy migration and application code that can read both legacy and new representations.
3. Backfill JSON fields for existing records from current structured data where available.
4. Enable validation checks in service layer and tests for JSON round-trip consistency.
5. Rollback strategy: application can ignore new JSON fields; migration rollback drops columns only if no dependent release requires them.

## Open Questions

- Should `WorkflowJson` and `RuleJson` be stored as `nvarchar(max)` or database-native JSON type where available?
- Do we enforce strict canonical formatting (property order/spacing) or semantic JSON equivalence only?
- Should API responses always include JSON fields, or gate them by version/flag for payload-size control?
