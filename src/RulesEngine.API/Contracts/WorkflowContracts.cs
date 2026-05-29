using RulesEngine.Application.Dtos;

namespace RulesEngine.API.Contracts;

public sealed record WorkflowRequest(
    WorkflowDto Workflow,
    int? SchemaVersion,
    bool CreateNewVersion = false);

public sealed record WorkflowResponse(
    Guid Id,
    WorkflowDto Workflow,
    int Version,
    int ActiveVersion,
    int LastVersion,
    bool IsActive,
    bool IsEnabled,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc);

public sealed record ValidationErrorResponse(int SchemaVersion, IReadOnlyCollection<string> Errors);

public sealed record ExecuteWorkflowRequest(
    bool DryRun,
    int? SchemaVersion,
    IReadOnlyList<RuleParameterDto> Inputs,
    IReadOnlyList<string>? IncludeStatuses);

public sealed record ExecuteWorkflowResponse(
    bool DryRun,
    int SchemaVersion,
    bool Persisted,
    bool WasSuccessful,
    IReadOnlyList<RuleResultDto> Results,
    IReadOnlyList<RuleStatusTransitionDto> RuleStatusTransitions,
    Guid? ExecutionId);

public sealed record ValidateWorkflowRequest(WorkflowDto Workflow);

public sealed record ValidateWorkflowResponse(
    bool IsValid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<RuleStatusTransitionDto> RuleStatusTransitions);

public sealed record ExecutionErrorResponse(string Code, string Message, int? SchemaVersion, IReadOnlyCollection<string>? Errors);
