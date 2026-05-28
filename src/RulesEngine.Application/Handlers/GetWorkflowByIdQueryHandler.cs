using System.Text.Json;
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

        var workflow = JsonSerializer.Deserialize<WorkflowDto>(record.RuleJson);
        if (workflow is null)
        {
            return null;
        }

        return new WorkflowDto
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
        };
    }
}
