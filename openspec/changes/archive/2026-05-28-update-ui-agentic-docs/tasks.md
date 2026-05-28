## 1. OpenSpec and Capability Alignment

- [x] 1.1 Validate `ui-editor` and `integrations` delta specs for strict OpenSpec formatting (`#### Scenario`, SHALL/MUST language, and modified requirement completeness).
- [x] 1.2 Confirm the capability scope in proposal/design remains documentation governance only (no runtime behavior changes).
- [x] 1.3 Run `openspec validate --change update-ui-agentic-docs --strict` and resolve any spec diagnostics.

## 2. RulesEngine.UI Agentic Documentation Updates

- [x] 2.1 Update workflow editor docs for `RulesEngine.UI` to state that it is a Blazor WebAssembly project and that LogicFlow compiled assets ship from `./src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/`.
- [x] 2.2 Document required runtime assets in the same guidance (`index.css`, `index.min.js`) and reference local markdown docs under `/src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/docs`.
- [x] 2.3 Add the standardized LogicFlow agent rules snippet with `BEGIN/END` markers and include package-role guidance plus local-doc-first policy.

## 3. Node Source and Capability Lookup Guidance

- [x] 3.1 Add node source documentation lookup path `node_modules/@logicflow/core/dist/docs/` to agent-focused docs.
- [x] 3.2 Add extension/layout docs lookup path `node_modules/@logicflow/core/dist/docs/tutorial/extension/` and explain when to use it.
- [x] 3.3 Ensure docs explicitly direct contributors/agents to prefer official built-in, extension, and layout capabilities before custom implementation.

## 4. Dependency Governance and Verification

- [x] 4.1 Add or verify wording that missing official LogicFlow packages require user confirmation before installation.
- [x] 4.2 Cross-check updated docs against current repository paths and project structure (`RulesEngine.UI`, `wwwroot`, `node_modules`).
- [x] 4.3 Re-run `openspec status --change update-ui-agentic-docs` and confirm the change is apply-ready.