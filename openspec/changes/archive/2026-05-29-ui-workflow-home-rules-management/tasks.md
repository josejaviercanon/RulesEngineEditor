## 1. Navigation and Shared UI Data Layer

- [x] 1.1 Add Home and Rules menu entries and route wiring in `src/RulesEngine.UI`, with Home configured as the default landing page.
- [x] 1.2 Create shared workflow list query/service logic that returns one row per workflow identity using active revision projection.
- [x] 1.3 Define shared UI view models for workflow grid rows, workflow version items, and nested rule grid rows.

## 2. Home Page Workflow Grid and Modals

- [x] 2.1 Implement Home page grid with columns Actions, Guid ID, latest version, workflow name, and enablement state.
- [x] 2.2 Implement New Workflow modal using Blazorise form controls with type-appropriate inputs, Save/Cancel actions, and client-side field validation.
- [x] 2.3 Wire New Workflow save flow to API, handle success/failure states, and refresh grid on successful save.
- [x] 2.4 Implement Edit Workflow modal with current workflow data, workflow versions list box, active-version selection action, and enablement controls.
- [x] 2.5 Wire edit actions for activation and enable/disable updates, surfacing backend validation responses in modal alerts/messages.

## 3. Rules Page and Nested Rule Management

- [x] 3.1 Implement Rules page workflow grid variant with row expand indicator and Rules-specific actions.
- [x] 3.2 Implement row-details nested rules grid with columns Actions, Guid ID, Name, and Version.
- [x] 3.3 Implement Create Rule action scoped to selected workflow and refresh nested grid after successful save.
- [x] 3.4 Implement Edit Rule modal for Name and Expression fields with Save/Cancel behavior and preserved form state on errors.
- [x] 3.5 Wire rule create/update requests to API and display expression validation feedback when validation fails.

## 4. API and Application Contract Alignment

- [x] 4.1 Verify existing API contracts support UI requirements for workflow list, version list, workflow activation, and enable/disable transitions; add additive endpoints/response fields if required.
- [x] 4.2 Ensure rule create/update handlers return structured validation details for expression failures consumable by UI.
- [x] 4.3 Add/update application-level mapping and DTO handling needed by new UI payloads without regressing existing clients.

## 5. Testing and Acceptance

- [x] 5.1 Add UI/component tests for default Home route, Home grid rendering, and modal create/edit workflow flows.
- [x] 5.2 Add tests for Rules page row expansion, nested rules grid rendering, and create/edit rule flows.
- [x] 5.3 Add integration tests covering workflow activation/enablement invariants exposed through UI-triggered API paths.
- [x] 5.4 Add integration tests for rule expression validation failure responses and no-mutation guarantees on invalid saves.
- [x] 5.5 Execute build/tests and verify acceptance checklist: Home default page, grid refresh on save, version activation behavior, and nested rules validation feedback.
