using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record ActivateRuleVersionCommand(Guid WorkflowId, Guid RuleGuidId, int Version) : IRequest<RuleDto?>;
