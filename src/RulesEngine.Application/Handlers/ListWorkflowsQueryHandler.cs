using System.Text.Json;
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
            .Select(record => JsonSerializer.Deserialize<WorkflowDto>(record.RuleJson) is { } workflow
                ? new WorkflowDto
            {
                    WorkflowName = workflow.WorkflowName,
                    RuleExpressionType = workflow.RuleExpressionType,
                    GlobalParams = workflow.GlobalParams,
                    Rules = workflow.Rules,
                    WorkflowsToInject = workflow.WorkflowsToInject,
                    Id = record.Id,
                    Version = record.Version,
                    IsActive = record.IsActive,
                    EffectiveFromUtc = record.EffectiveFromUtc,
                    EffectiveToUtc = record.EffectiveToUtc
                }
                : new WorkflowDto
                {
                    Id = record.Id,
                    WorkflowName = record.Name,
                    Version = record.Version,
                    IsActive = record.IsActive,
                    EffectiveFromUtc = record.EffectiveFromUtc,
                    EffectiveToUtc = record.EffectiveToUtc
                })
            .ToArray();
    }
}
