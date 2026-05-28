using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class ListWorkflowsQueryHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<ListWorkflowsQuery, IReadOnlyCollection<WorkflowDto>>
{
    public async Task<IReadOnlyCollection<WorkflowDto>> Handle(ListWorkflowsQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var records = await workflowRepository.ListAsync(cancellationToken);
        return records
            .Select(record => new WorkflowDto
            {
                Id = record.Id,
                Name = record.Name,
                Expression = record.Expression,
                RuleJson = record.RuleJson,
                Version = record.Version,
                IsActive = record.IsActive,
                EffectiveFromUtc = record.EffectiveFromUtc,
                EffectiveToUtc = record.EffectiveToUtc
            })
            .ToArray();
    }
}
