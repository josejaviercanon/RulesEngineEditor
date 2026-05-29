using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record ActivateWorkflowVersionCommand(Guid Id, int Version) : IRequest<WorkflowDto?>;