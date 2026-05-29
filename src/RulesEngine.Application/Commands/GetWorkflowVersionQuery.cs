using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record GetWorkflowVersionQuery(Guid Id, int Version) : IRequest<WorkflowDto?>;