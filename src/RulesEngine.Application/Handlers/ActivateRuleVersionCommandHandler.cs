using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class ActivateRuleVersionCommandHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<ActivateRuleVersionCommand, RuleDto?>
{
    public async Task<RuleDto?> Handle(ActivateRuleVersionCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var activated = await workflowRepository.ActivateRuleVersionAsync(
            request.WorkflowId,
            request.RuleGuidId,
            request.Version,
            cancellationToken);

        return activated is null ? null : WorkflowDtoProjection.MapRule(activated);
    }
}
