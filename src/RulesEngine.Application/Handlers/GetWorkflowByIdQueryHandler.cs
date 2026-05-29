using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class GetWorkflowByIdQueryHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<GetWorkflowByIdQuery, WorkflowDto?>
{
    public async Task<WorkflowDto?> Handle(GetWorkflowByIdQuery request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var record = await workflowRepository.GetByIdAsync(request.Id, cancellationToken, request.Mode);
        if (record is null)
        {
            return null;
        }

        return await WorkflowDtoProjection.BuildAsync(record, workflowRepository, request.Mode, cancellationToken);
    }
}
