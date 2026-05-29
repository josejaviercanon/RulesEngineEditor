## MODIFIED Requirements

### Requirement: Rules page supports expandable workflow-to-rules management
The UI SHALL provide a Rules page that reuses workflow grid context, includes row expansion for workflow rules, and supports rule create/edit/delete/validate operations through modal dialogs and row actions. The Rules page workflow main grid SHALL show `Active Version` independently from `Last Version`, and the nested rules grid SHALL show active-only rows per rule identity.

#### Scenario: Rules page workflow main grid shows required version columns
- **WHEN** the user opens the Rules page workflow grid
- **THEN** each workflow row shows columns including Actions, Guid ID, Active Version, Last Version, Name, and enablement state
- **AND** Active Version appears immediately after Guid ID
- **AND** Last Version continues to represent total retained workflow versions

#### Scenario: Rules page workflow row expansion shows active-only nested rules grid
- **WHEN** the user expands a workflow row on Rules page
- **THEN** a detail grid is shown with columns Actions, Guid ID, Name, Active Version, and Last Version
- **AND** Active Version appears immediately before Last Version
- **AND** the grid includes only active rule revisions for the selected workflow context
- **AND** the grid includes at most one row per `RuleGuidId`

#### Scenario: Create rule for selected workflow
- **WHEN** the user clicks create-rule action for a workflow and saves valid rule data
- **THEN** a new rule revision is persisted for the selected workflow context
- **AND** the nested rules grid refreshes

#### Scenario: Edit rule validates expression and preserves modal state on failure
- **WHEN** the user edits rule Name/Expression and submits invalid expression content
- **THEN** the UI shows validation information returned by the system
- **AND** the modal remains open with user-entered values for correction

#### Scenario: Edit rule modal supports version list and active toggle parity
- **WHEN** the user opens Edit for a rule from the nested rules grid
- **THEN** the modal shows a versions list for the selected `RuleGuidId`
- **AND** the modal shows an active-version toggle for the selected version
- **AND** clicking Save updates only the currently selected version unless create-new-version is explicitly requested

#### Scenario: Changing selected rule version reloads full modal model
- **WHEN** the user selects a different version in the modal versions list
- **THEN** the modal reloads expression and all editable rule properties for that selected persisted version
- **AND** the active-version toggle state updates to match the selected version

#### Scenario: Saving active-version change leaves exactly one active version
- **WHEN** the user changes active-version state and clicks Save
- **THEN** the selected version becomes active only when requested
- **AND** any previously active version for the same `RuleGuidId` is no longer active after save

#### Scenario: Rule row actions include delete and validate
- **WHEN** the nested rules grid is rendered
- **THEN** the Actions column includes Edit, Delete, and Validate actions in that order
- **AND** Delete prompts a yes/no confirmation dialog before deletion
- **AND** Validate runs expression validation and shows whether the expression is valid or includes validation errors

#### Scenario: Workflow row actions include validate workflow
- **WHEN** the workflow grid is rendered on the Rules page
- **THEN** the Actions column includes a Validate action after the New Rule action
- **AND** Validate runs full workflow validation and shows whether the workflow is valid or includes validation errors
