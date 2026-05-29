## MODIFIED Requirements

### Requirement: WorkflowDto represents the full RulesEngine workflow model
The system SHALL define workflow and rule DTO contracts that represent rule identity/version metadata required for historical retrieval and activation workflows. The DTO model MUST support `RuleGuidId`, integer `Version`, and `IsActive` rule-state fields in addition to existing rule expression fields.

#### Scenario: RuleDto exposes version identity metadata
- **WHEN** a `RuleDto` is materialized for workflow management responses
- **THEN** it includes `RuleGuidId` (Guid), `Version` (int), and `IsActive` (bool) with values matching persisted rule revision state

#### Scenario: WorkflowDto retrieval avoids duplicate logical rules by mode
- **WHEN** a workflow is returned in active-only or latest-per-rule mode
- **THEN** the `Rules` collection includes at most one entry per `RuleGuidId` according to the selected retrieval mode

#### Scenario: WorkflowDto retrieval can include full history
- **WHEN** a workflow is returned in history-inclusive mode
- **THEN** the response can represent all retained revisions for each `RuleGuidId` while preserving each rule revision's `Version` and `IsActive` state
