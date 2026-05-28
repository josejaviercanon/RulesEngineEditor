using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class UpdateWorkflowCommandHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<UpdateWorkflowCommand, WorkflowDto?>
{
    public async Task<WorkflowDto?> Handle(UpdateWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var updated = await workflowRepository.UpdateAsync(request.Id, new WorkflowRecord
        {
            Name = request.Name,
            Expression = request.Expression,
            RuleJson = request.RuleJson,
            Version = request.Version,
            IsActive = request.IsActive,
            EffectiveFromUtc = request.EffectiveFromUtc,
            EffectiveToUtc = request.EffectiveToUtc
        }, cancellationToken);

        if (updated is null)
        {
            return null;
        }

        return new WorkflowDto
        {
            Id = updated.Id,
            Name = updated.Name,
            Expression = updated.Expression,
            RuleJson = updated.RuleJson,
            Version = updated.Version,
            IsActive = updated.IsActive,
            EffectiveFromUtc = updated.EffectiveFromUtc,
            EffectiveToUtc = updated.EffectiveToUtc
        };
    }
}
