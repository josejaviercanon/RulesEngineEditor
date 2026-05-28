## Why

Agentic development guidance for the workflow editor is currently fragmented, which makes it easy to miss local LogicFlow capabilities and reimplement features that already exist. The UI project is Blazor WebAssembly and ships a compiled LogicFlow bundle in `wwwroot`, so docs must explicitly direct contributors and agents to the local assets and docs first.

## What Changes

- Add OpenSpec requirement deltas that define agent-facing documentation rules for the workflow UI editor in `RulesEngine.UI`.
- Document the Blazor WebAssembly distribution path for LogicFlow runtime assets: `./src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/` (`index.css`, `index.min.js`).
- Document the local Markdown documentation path consumed by the UI project: `/src/RulesEngine.UI/wwwroot/js/logicflow-2.2.3/docs`.
- Add the source-of-truth Node package documentation path and package-role guidance for `@logicflow/core`, `@logicflow/extension`, and `@logicflow/layout`.
- Require an explicit "check local docs first" policy and "ask before installing official package" policy for agent-driven feature work.

## Capabilities

### New Capabilities
- `agentic-ui-doc-governance`: Capability for governing agent-facing LogicFlow documentation usage in the Blazor workflow editor.

### Modified Capabilities
- `ui-editor`: Extend workflow editor requirements to include normative agentic documentation paths and usage rules for local LogicFlow assets.
- `integrations`: Extend integration requirements to cover package-role and extension/layout documentation lookup requirements for LogicFlow.

## Impact

- Affected specs: `openspec/specs/ui-editor/spec.md`, `openspec/specs/integrations/spec.md`.
- Affected docs (implementation target): UI docs and OpenSpec change docs that guide contributors/agents working on `RulesEngine.UI` workflow editor features.
- Dependencies and decision flow: `@logicflow/core` docs are treated as first-class local references before introducing custom implementation work.