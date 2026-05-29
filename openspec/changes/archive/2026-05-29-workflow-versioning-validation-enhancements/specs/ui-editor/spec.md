## MODIFIED Requirements

### Requirement: Home page is the default workflow management entry point
The UI SHALL open to the Home page by default and SHALL present a workflow grid with one row per workflow identity showing active revision metadata and operator actions.

#### Scenario: Home route is the default page
- **WHEN** a user opens the RulesEngine.UI application
- **THEN** the Home page is shown as the initial route
- **AND** the workflow grid is loaded without requiring manual navigation

#### Scenario: Home workflow grid shows required columns
- **WHEN** the Home page workflow list is rendered
- **THEN** each row shows Actions, workflow Guid ID, Active Version, latest workflow version, workflow name, and enablement state
- **AND** Active Version appears immediately after Guid ID
- **AND** each row represents the active revision projection for a workflow identity

#### Scenario: Create workflow from Home actions
- **WHEN** the user clicks New in Home actions and saves a valid modal form
- **THEN** a new workflow is created without requiring system-only fields in user input
- **AND** the new workflow enablement default is OFF
- **AND** the grid refreshes to show the new workflow row

#### Scenario: Edit workflow from Home actions with version controls
- **WHEN** the user opens Edit for a workflow row
- **THEN** a modal shows editable workflow data and a version list for that workflow identity
- **AND** clicking Save updates only the currently selected version in the version list
- **AND** clicking Create New Version opens a yes/no confirmation dialog before creating a new version
- **AND** the user can activate a selected version and set enablement according to lifecycle constraints

### Requirement: Rules page supports expandable workflow-to-rules management
The UI SHALL provide a Rules page that reuses workflow grid context, includes row expansion for workflow rules, and supports rule create/edit/delete/validate operations through modal dialogs and row actions.

#### Scenario: Rules page workflow row expansion shows nested rules grid
- **WHEN** the user opens Rules and expands a workflow row
- **THEN** a detail grid is shown for that workflow with columns Actions, Guid ID, Name, and Version

#### Scenario: Create rule for selected workflow
- **WHEN** the user clicks create-rule action for a workflow and saves valid rule data
- **THEN** a new rule revision is persisted for the selected workflow context
- **AND** the nested rules grid refreshes

#### Scenario: Edit rule validates expression and preserves modal state on failure
- **WHEN** the user edits rule Name/Expression and submits invalid expression content
- **THEN** the UI shows validation information returned by the system
- **AND** the modal remains open with user-entered values for correction

#### Scenario: Edit rule saves selected version unless new-version action is confirmed
- **WHEN** the user opens Edit for a rule version from the nested rules grid
- **THEN** clicking Save updates only the selected rule version
- **AND** clicking Create New Version opens a yes/no confirmation dialog before creating a new rule version

#### Scenario: Rule row actions include delete and validate
- **WHEN** the nested rules grid is rendered
- **THEN** the Actions column includes Edit, Delete, and Validate actions in that order
- **AND** Delete prompts a yes/no confirmation dialog before deletion
- **AND** Validate runs expression validation and shows whether the expression is valid or includes validation errors

#### Scenario: Workflow row actions include validate workflow
- **WHEN** the workflow grid is rendered on the Rules page
- **THEN** the Actions column includes a Validate action after the New Rule action
- **AND** Validate runs full workflow validation and shows whether the workflow is valid or includes validation errors
