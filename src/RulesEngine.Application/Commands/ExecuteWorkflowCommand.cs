using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record ExecuteWorkflowCommand(
    Guid WorkflowId,
    bool DryRun,
    int? SchemaVersion,
    IReadOnlyList<RuleParameterDto> Inputs,
    IReadOnlyList<string>? IncludeStatuses) : IRequest<ExecuteWorkflowResultDto>;
