using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record UpdateWorkflowCommand(
    Guid Id,
    string Name,
    string Expression,
    string RuleJson,
    int Version,
    bool IsActive,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc) : IRequest<WorkflowDto?>;
