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

        var records = await workflowRepository.ListAsync(cancellationToken, request.Mode, request.IsEnabled);
        var projected = new List<WorkflowDto>(records.Count);
        foreach (var record in records)
        {
            projected.Add(await WorkflowDtoProjection.BuildAsync(record, workflowRepository, request.Mode, cancellationToken));
        }

        return projected;
    }
}
