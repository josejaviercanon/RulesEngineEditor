using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class SetWorkflowVersionEnabledCommandHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<SetWorkflowVersionEnabledCommand, WorkflowDto?>
{
    public async Task<WorkflowDto?> Handle(SetWorkflowVersionEnabledCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var target = await workflowRepository.GetVersionAsync(
            request.Id,
            request.Version,
            cancellationToken,
            WorkflowRuleQueryMode.ActiveOnly,
            null);

        if (target is null)
        {
            return null;
        }

        if (!target.IsActive)
        {
            throw new InvalidOperationException("Only the active workflow version can be enabled or disabled.");
        }

        var updated = await workflowRepository.SetVersionEnabledAsync(
            request.Id,
            request.Version,
            request.IsEnabled,
            cancellationToken);

        if (updated is null)
        {
            return null;
        }

        return await WorkflowDtoProjection.BuildAsync(
            updated,
            workflowRepository,
            WorkflowRuleQueryMode.ActiveOnly,
            cancellationToken);
    }
}
