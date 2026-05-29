## MODIFIED Requirements

### Requirement: WorkflowDto represents the full RulesEngine workflow model
The system SHALL define workflow and rule DTO contracts that represent workflow and rule identity/version metadata required for historical retrieval, activation workflows, and operational enablement controls. The DTO model MUST support workflow-level `IsEnabled` (bool), `Comments` (string, max length 4000), and `WorkflowJson` (string JSON containing the full workflow document), and rule-level `RuleGuidId`, integer `Version`, `IsActive`, `Status`, and `RuleJson` (string JSON containing the rule representation including plain-text `Expression`) fields in addition to existing fields. `Status` MUST be constrained to `draft`, `failed`, `disabled`, or `production`.

#### Scenario: WorkflowDto exposes enablement, comments, and canonical workflow JSON
- **WHEN** a `WorkflowDto` is materialized for workflow management responses
- **THEN** it includes `IsEnabled` (bool), `Comments` (string, nullable, max 4000 characters), and `WorkflowJson` (string JSON) with values matching persisted revision state

#### Scenario: RuleDto exposes version identity metadata and canonical rule JSON
- **WHEN** a `RuleDto` is materialized for workflow management responses
- **THEN** it includes `RuleGuidId` (Guid), `Version` (int), `IsActive` (bool), `Status` (string enum), and `RuleJson` (string JSON including plain-text `Expression`) with values matching persisted rule revision state

#### Scenario: WorkflowDto retrieval avoids duplicate logical rules by mode
- **WHEN** a workflow is returned in active-only or latest-per-rule mode
- **THEN** the `Rules` collection includes at most one entry per `RuleGuidId` according to the selected retrieval mode

#### Scenario: WorkflowDto retrieval can include full history
- **WHEN** a workflow is returned in history-inclusive mode
- **THEN** the response can represent all retained revisions for each `RuleGuidId` while preserving each rule revision's `Version`, `IsActive`, `Status`, and `RuleJson` state

#### Scenario: ScopedParamDto carries name and expression
- **WHEN** a `ScopedParamDto` is constructed
- **THEN** it exposes `Name` (string) and `Expression` (string)

#### Scenario: RuleActionsDto carries success and failure action info
- **WHEN** a `RuleActionsDto` is constructed
- **THEN** it exposes `OnSuccess` (ActionInfoDto, nullable) and `OnFailure` (ActionInfoDto, nullable)

#### Scenario: ActionInfoDto carries action name and context
- **WHEN** an `ActionInfoDto` is constructed
- **THEN** it exposes `Name` (string) and `Context` (Dictionary<string, JsonElement>)
