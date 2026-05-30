## 1. Data Model and Backend Contracts

- [x] 1.1 Add `ExecuteOrder` to rule domain/entity models, DTOs, mapping profiles, and API contracts
- [x] 1.2 Create and apply EF Core migration to add `ExecuteOrder` column to rules table with safe default/backfill behavior
- [x] 1.3 Update workflow/rule query handlers to return rules ordered by `ExecuteOrder` with deterministic tie-breaks
- [x] 1.4 Update command handlers/save services to accept and persist `ExecuteOrder` updates from UI payloads

## 2. Save Pipeline and Validation Flow

- [x] 2.1 Implement canonical visual-model-to-domain mapping that regenerates `WorkflowJson` and each `RuleJson` on save
- [x] 2.2 Add JSON correctness guardrails that block persistence when generated workflow/rule JSON is syntactically invalid
- [x] 2.3 Add dry-run validation pre-check in save workflow and return warning decision contract (`save-as-draft` or `cancel`) on validation errors
- [x] 2.4 Implement draft override save path that persists with warnings while returning validation diagnostics to the UI

## 3. Shared Rule Editing UX

- [x] 3.1 Refactor/introduce shared rule edit modal model/component used by both Rules Page and Visual Editor
- [x] 3.2 Add editable numeric `ExecuteOrder` field to create/edit rule modal with input validation and binding
- [x] 3.3 Ensure modal parity for existing RulesEngine rule options/types and version selection behavior after refactor

## 4. Visual Editor UI and LogicFlow Interactions

- [x] 4.1 Add `Visual Editor` route/menu entry that initializes a blank LogicFlow canvas by default
- [x] 4.2 Add toolbar actions `Load` and `Save` to the Visual Editor with command wiring
- [x] 4.3 Implement `Load` modal with active workflow versions list box and `Accept`/`Cancel` actions
- [x] 4.4 Implement workflow hydration into diagram state and rule-node graph rendering from selected workflow
- [x] 4.5 Implement node create/link/edit interactions and open shared rule modal from node edit actions

## 5. Rules Page Integration

- [x] 5.1 Add `Visual Editor` button to Rules workflow action column and pass selected workflow context for auto-load
- [x] 5.2 Update Rules nested detail grid to include and sort by `ExecuteOrder`
- [x] 5.3 Ensure Visual Editor navigation auto-loads selected workflow and gracefully falls back to blank diagram when context is missing

## 6. Validation/Draft UX and Messaging

- [x] 6.1 Add warning dialog for save validation failures with explicit `Save as Draft` and `Cancel` actions
- [x] 6.2 Keep editor state intact when user cancels after warnings
- [x] 6.3 Surface returned warning/validation diagnostics in UI after draft saves for remediation

## 7. Testing and Verification

- [x] 7.1 Add backend unit/integration tests for `ExecuteOrder` persistence, ordering queries, and deterministic save normalization
- [x] 7.2 Add tests for save pre-validation decision flow (valid save, warning+cancel, warning+draft save)
- [ ] 7.3 Add tests for canonical JSON regeneration and malformed JSON rejection behavior
- [ ] 7.4 Add UI/component tests for Visual Editor load/save flows, node/property editing, and Rules Page Visual Editor navigation
- [x] 7.5 Run `openspec validate --strict` and resolve all artifact validation issues
