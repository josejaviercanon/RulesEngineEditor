using MediatR;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Models;

namespace RulesEngine.Application.Commands;

public sealed record GetWorkflowVersionQuery(
	Guid Id,
	int Version,
	WorkflowRuleQueryMode Mode = WorkflowRuleQueryMode.ActiveOnly) : IRequest<WorkflowDto?>;