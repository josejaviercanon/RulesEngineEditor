using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record GetWorkflowByIdQuery(Guid Id) : IRequest<WorkflowDto?>;
