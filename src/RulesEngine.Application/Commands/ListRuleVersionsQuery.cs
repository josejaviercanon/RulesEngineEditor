using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record ListRuleVersionsQuery(Guid WorkflowId, Guid RuleGuidId) : IRequest<IReadOnlyCollection<RuleDto>>;
