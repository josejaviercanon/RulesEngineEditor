using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record ListWorkflowVersionsQuery(Guid Id) : IRequest<IReadOnlyCollection<WorkflowDto>>;