using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record ValidateWorkflowCommand(
    WorkflowDto Workflow) : IRequest<WorkflowValidationDto>;
