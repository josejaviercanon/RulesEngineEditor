using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class DeleteWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IRulesEngineWorkflowService rulesEngineWorkflowService)
    : IRequestHandler<DeleteWorkflowCommand, bool>
{
    public async Task<bool> Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var deleted = await workflowRepository.DeleteAsync(request.Id, cancellationToken);
        if (deleted)
        {
            rulesEngineWorkflowService.EvictWorkflow(request.Id);
        }

        return deleted;
    }
}
