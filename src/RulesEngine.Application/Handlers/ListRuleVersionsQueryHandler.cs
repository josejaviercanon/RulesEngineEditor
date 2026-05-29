using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class ListRuleVersionsQueryHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<ListRuleVersionsQuery, IReadOnlyCollection<RuleDto>>
{
    public async Task<IReadOnlyCollection<RuleDto>> Handle(ListRuleVersionsQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var versions = await workflowRepository.ListRuleVersionsAsync(request.WorkflowId, request.RuleGuidId, cancellationToken);
        return versions
            .Select(WorkflowDtoProjection.MapRule)
            .OrderBy(rule => rule.Version)
            .ToArray();
    }
}
