## Context

`RulesEngine.UI` is a Blazor WebAssembly project with workflow-canvas behavior implemented through JS interop and bundled frontend assets in `wwwroot`. LogicFlow is consumed from a compiled distribution under `./src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/`, and local markdown docs are available under `/src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/docs`.

Today, contributor and agent guidance does not consistently enforce local-doc-first discovery, package-role awareness, or package-installation guardrails for LogicFlow-related feature work. This creates avoidable risk of duplicate implementation and dependency drift.

## Goals / Non-Goals

**Goals:**
- Define normative requirements that make local LogicFlow bundle/docs the first lookup path for UI editor changes.
- Define package role guidance for `@logicflow/core`, `@logicflow/extension`, and `@logicflow/layout`.
- Require agent-facing docs to include explicit guardrails for official capability reuse and pre-installation user confirmation.
- Align docs with current Blazor WASM runtime delivery paths used by `RulesEngine.UI`.

**Non-Goals:**
- Rewriting workflow editor runtime code or replacing existing JS interop contracts.
- Upgrading LogicFlow runtime versions in this change.
- Introducing new UI behavior beyond documentation and specification updates.

## Decisions

1. Add requirement deltas instead of introducing a new standalone platform capability.
- Decision: Apply `MODIFIED Requirements` to existing `ui-editor` and `integrations` capabilities.
- Rationale: The new behavior is governance/documentation of existing editor and integration contracts, not a separate runtime domain.
- Alternative considered: Creating a new capability only for agent docs. Rejected to avoid fragmenting ownership from the editor/integration contracts it governs.

2. Treat Blazor WASM-distributed docs/assets as authoritative runtime references.
- Decision: Requirements will explicitly name `./src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/` (`index.css`, `index.min.js`) and `/src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/docs`.
- Rationale: Contributors and agents need deterministic local paths that match shipped UI behavior.
- Alternative considered: Only referencing `node_modules` docs. Rejected because the deployed app consumes compiled assets in `wwwroot`.

3. Encode LogicFlow package-role and extension/layout lookup policy in spec scenarios.
- Decision: Requirements will include package roles and the path `node_modules/@logicflow/core/dist/docs/tutorial/extension/` for extension/layout docs.
- Rationale: This ensures design/implementation choices favor official capabilities before custom implementations.
- Alternative considered: Keep this as informal README text. Rejected because it is easy to bypass and harder to validate.

4. Require explicit install-confirmation guardrail for official packages.
- Decision: Specs will mandate asking the user before installing missing official LogicFlow packages.
- Rationale: Matches controlled dependency practices and prevents silent dependency additions in agentic workflows.
- Alternative considered: Allow auto-install during implementation. Rejected due to governance and auditability concerns.

## Risks / Trade-offs

- [Path drift risk] Future LogicFlow version bumps can invalidate hardcoded paths -> Mitigation: include versioned-path update in release checklist and keep spec deltas synchronized.
- [Doc duplication risk] Rules may be duplicated across OpenSpec and repo docs -> Mitigation: keep OpenSpec normative and reference shared snippets for implementation docs.
- [Over-constrained workflow risk] Strict install confirmation may slow rapid prototyping -> Mitigation: scope the rule to official package installation events only.
- [Ambiguity risk] Contributors may not know whether to use `wwwroot/docs` or `node_modules/docs` first -> Mitigation: specify primary/secondary lookup order in requirements.