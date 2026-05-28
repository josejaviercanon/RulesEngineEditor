using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class DeleteWorkflowCommandHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<DeleteWorkflowCommand, bool>
{
    public Task<bool> Handle(DeleteWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return workflowRepository.DeleteAsync(request.Id, cancellationToken);
    }
}
