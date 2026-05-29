using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class GetWorkflowVersionQueryHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<GetWorkflowVersionQuery, WorkflowDto?>
{
    public async Task<WorkflowDto?> Handle(GetWorkflowVersionQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var record = await workflowRepository.GetVersionAsync(request.Id, request.Version, cancellationToken, request.Mode);
        if (record is null)
        {
            return null;
        }

        return await WorkflowDtoProjection.BuildAsync(record, workflowRepository, request.Mode, cancellationToken);
    }
}