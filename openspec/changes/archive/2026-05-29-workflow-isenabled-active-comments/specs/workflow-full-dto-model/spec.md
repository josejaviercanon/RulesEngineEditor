## MODIFIED Requirements

### Requirement: WorkflowDto represents the full RulesEngine workflow model
The system SHALL define workflow and rule DTO contracts that represent workflow and rule identity/version metadata required for historical retrieval, activation workflows, and operational enablement controls. The DTO model MUST support workflow-level `IsEnabled` (bool) and `Comments` (string, max length 4000), and rule-level `RuleGuidId`, integer `Version`, and `IsActive` fields in addition to existing fields.

#### Scenario: WorkflowDto exposes enablement and comments metadata
- **WHEN** a `WorkflowDto` is materialized for workflow management responses
- **THEN** it includes `IsEnabled` (bool) and `Comments` (string, nullable, max 4000 characters) with values matching persisted revision state

#### Scenario: RuleDto exposes version identity metadata
- **WHEN** a `RuleDto` is materialized for workflow management responses
- **THEN** it includes `RuleGuidId` (Guid), `Version` (int), and `IsActive` (bool) with values matching persisted rule revision state

#### Scenario: WorkflowDto retrieval avoids duplicate logical rules by mode
- **WHEN** a workflow is returned in active-only or latest-per-rule mode
- **THEN** the `Rules` collection includes at most one entry per `RuleGuidId` according to the selected retrieval mode

#### Scenario: WorkflowDto retrieval can include full history
- **WHEN** a workflow is returned in history-inclusive mode
- **THEN** the response can represent all retained revisions for each `RuleGuidId` while preserving each rule revision's `Version` and `IsActive` state

#### Scenario: ScopedParamDto carries name and expression
- **WHEN** a `ScopedParamDto` is constructed
- **THEN** it exposes `Name` (string) and `Expression` (string)

#### Scenario: RuleActionsDto carries success and failure action info
- **WHEN** a `RuleActionsDto` is constructed
- **THEN** it exposes `OnSuccess` (ActionInfoDto, nullable) and `OnFailure` (ActionInfoDto, nullable)

#### Scenario: ActionInfoDto carries action name and context
- **WHEN** an `ActionInfoDto` is constructed
- **THEN** it exposes `Name` (string) and `Context` (Dictionary<string, JsonElement>)

### Requirement: AutoMapper profiles bridge WorkflowDto and RulesEngine.Models.Workflow
The system SHALL provide AutoMapper mapping profiles that convert `WorkflowDto` → `RulesEngine.Models.Workflow` and `RulesEngine.Models.Workflow` → `WorkflowDto` without data loss for all supported fields, including workflow `IsEnabled` and `Comments`.

#### Scenario: Mapping WorkflowDto to Workflow preserves all workflow and rule fields
- **WHEN** AutoMapper maps a `WorkflowDto` with nested `RuleDto` objects to `RulesEngine.Models.Workflow`
- **THEN** the resulting model preserves `IsEnabled`, `Comments`, and all supported nested rule fields with equivalent values

#### Scenario: Mapping Workflow to WorkflowDto preserves all workflow and rule fields
- **WHEN** AutoMapper maps a `RulesEngine.Models.Workflow` with nested `Rule` objects to `WorkflowDto`
- **THEN** the resulting DTO preserves `IsEnabled`, `Comments`, and all supported nested rule fields with equivalent values
