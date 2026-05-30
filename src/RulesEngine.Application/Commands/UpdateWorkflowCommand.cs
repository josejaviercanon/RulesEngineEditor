using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record UpdateWorkflowCommand(
    Guid Id,
    WorkflowDto Workflow,
    int? SchemaVersion,
    bool CreateNewVersion = false,
    bool SaveAsDraft = false) : IRequest<WorkflowDto?>;
