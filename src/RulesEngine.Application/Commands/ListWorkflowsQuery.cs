using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record ListWorkflowsQuery : IRequest<IReadOnlyCollection<WorkflowDto>>;
