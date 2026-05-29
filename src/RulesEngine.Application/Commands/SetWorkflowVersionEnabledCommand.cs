using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record SetWorkflowVersionEnabledCommand(
    Guid Id,
    int Version,
    bool IsEnabled) : IRequest<WorkflowDto?>;
