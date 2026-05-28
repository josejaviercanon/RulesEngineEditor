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

        var record = await workflowRepository.GetByIdAsync(request.Id, cancellationToken);
        if (record is null)
        {
            return null;
        }

        return new WorkflowDto
        {
            Id = record.Id,
            Name = record.Name,
            Expression = record.Expression,
            RuleJson = record.RuleJson,
            Version = record.Version,
            IsActive = record.IsActive,
            EffectiveFromUtc = record.EffectiveFromUtc,
            EffectiveToUtc = record.EffectiveToUtc
        };
    }
}
