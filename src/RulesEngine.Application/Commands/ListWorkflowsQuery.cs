using MediatR;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Models;

namespace RulesEngine.Application.Commands;

public sealed record ListWorkflowsQuery(
	WorkflowRuleQueryMode Mode = WorkflowRuleQueryMode.ActiveOnly) : IRequest<IReadOnlyCollection<WorkflowDto>>;
