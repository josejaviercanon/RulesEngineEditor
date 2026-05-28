using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record CreateWorkflowCommand(
    WorkflowDto Workflow,
    int? SchemaVersion) : IRequest<WorkflowDto>;
