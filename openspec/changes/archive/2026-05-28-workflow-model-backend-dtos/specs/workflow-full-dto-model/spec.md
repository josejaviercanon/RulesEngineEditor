## ADDED Requirements

### Requirement: WorkflowDto represents the full RulesEngine workflow model
The system SHALL define a `WorkflowDto` in `RulesEngine.Application.Dtos` that fully mirrors the `RulesEngine.Models.Workflow` class, embedding typed collections for global parameters and rules rather than a raw JSON string.

#### Scenario: WorkflowDto carries all top-level workflow fields
- **WHEN** a `WorkflowDto` instance is constructed
- **THEN** it exposes `Id` (Guid, persistence identity), `WorkflowName` (string), `RuleExpressionType` (enum), `GlobalParams` (IReadOnlyList\<ScopedParamDto\>), `Rules` (IReadOnlyList\<RuleDto\>), and `WorkflowsToInject` (IReadOnlyList\<string\>)

#### Scenario: RuleDto carries all rule fields including nested rules
- **WHEN** a `RuleDto` instance is constructed
- **THEN** it exposes `RuleName`, `Operator`, `ErrorMessage`, `Enabled`, `RuleExpressionType`, `Expression`, `SuccessEvent`, `LocalParams` (IReadOnlyList\<ScopedParamDto\>), `Rules` (IReadOnlyList\<RuleDto\>), `Actions` (RuleActionsDto), `WorkflowsToInject` (IReadOnlyList\<string\>), and `Properties` (Dictionary\<string, string\>)

#### Scenario: ScopedParamDto carries name and expression
- **WHEN** a `ScopedParamDto` is constructed
- **THEN** it exposes `Name` (string) and `Expression` (string)

#### Scenario: RuleActionsDto carries success and failure action info
- **WHEN** a `RuleActionsDto` is constructed
- **THEN** it exposes `OnSuccess` (ActionInfoDto, nullable) and `OnFailure` (ActionInfoDto, nullable)

#### Scenario: ActionInfoDto carries action name and context
- **WHEN** an `ActionInfoDto` is constructed
- **THEN** it exposes `Name` (string) and `Context` (Dictionary\<string, JsonElement\>)

### Requirement: AutoMapper profiles bridge WorkflowDto and RulesEngine.Models.Workflow
The system SHALL provide AutoMapper mapping profiles that convert `WorkflowDto` → `RulesEngine.Models.Workflow` and `RulesEngine.Models.Workflow` → `WorkflowDto` without data loss for all supported fields.

#### Scenario: Mapping WorkflowDto to Workflow preserves all rule fields
- **WHEN** AutoMapper maps a `WorkflowDto` with nested `RuleDto` objects to `RulesEngine.Models.Workflow`
- **THEN** the resulting `Workflow.Rules` contains `Rule` objects with identical `RuleName`, `Expression`, `LocalParams`, `Actions`, `Operator`, `Enabled`, `SuccessEvent`, and nested `Rules`

#### Scenario: Mapping Workflow to WorkflowDto preserves all rule fields
- **WHEN** AutoMapper maps a `RulesEngine.Models.Workflow` with nested `Rule` objects to `WorkflowDto`
- **THEN** the resulting `WorkflowDto.Rules` contains `RuleDto` objects with identical field values

### Requirement: RuleResultDto represents per-rule execution output
The system SHALL define a `RuleResultDto` that carries the execution outcome for a single rule, including nested child results for rules with sub-rules.

#### Scenario: RuleResultDto carries rule outcome fields
- **WHEN** a `RuleResultDto` is constructed from a `RuleResultTree`
- **THEN** it exposes `RuleName` (string), `IsSuccess` (bool), `ExceptionMessage` (string, nullable), `SuccessEvent` (string, nullable), `ActionOutput` (string, nullable, serialized), and `ChildResults` (IReadOnlyList\<RuleResultDto\>)

### Requirement: RuleParameterDto represents a named test input
The system SHALL define a `RuleParameterDto` that carries a named input for workflow test execution.

#### Scenario: RuleParameterDto carries name and JSON value
- **WHEN** a `RuleParameterDto` is constructed
- **THEN** it exposes `Name` (string, required) and `ValueJson` (string, required — a JSON-serialized value to be deserialized as a dynamic object for the RulesEngine)
