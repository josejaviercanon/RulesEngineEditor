using MediatR;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Models;

namespace RulesEngine.Application.Commands;

public sealed record ListWorkflowVersionsQuery(
	Guid Id,
	WorkflowRuleQueryMode Mode = WorkflowRuleQueryMode.ActiveOnly,
	bool? IsEnabled = null) : IRequest<IReadOnlyCollection<WorkflowDto>>;