using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class ListWorkflowVersionsQueryHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<ListWorkflowVersionsQuery, IReadOnlyCollection<WorkflowDto>>
{
    public async Task<IReadOnlyCollection<WorkflowDto>> Handle(ListWorkflowVersionsQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var records = await workflowRepository.ListVersionsAsync(request.Id, cancellationToken, request.Mode);
        var projected = new List<WorkflowDto>(records.Count);
        foreach (var record in records)
        {
            projected.Add(await WorkflowDtoProjection.BuildAsync(record, workflowRepository, request.Mode, cancellationToken));
        }

        return projected
            .OrderBy(workflow => workflow.Version)
            .ToArray();
    }
}