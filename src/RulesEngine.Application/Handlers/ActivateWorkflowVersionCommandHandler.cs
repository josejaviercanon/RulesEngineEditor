using System.Text.Json;
using AutoMapper;
using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Repositories;
using RulesEngine.Models;

namespace RulesEngine.Application.Handlers;

public sealed class ActivateWorkflowVersionCommandHandler(
    IWorkflowRepository workflowRepository,
    IRulesEngineWorkflowService rulesEngineWorkflowService,
    IMapper mapper)
    : IRequestHandler<ActivateWorkflowVersionCommand, WorkflowDto?>
{
    public async Task<WorkflowDto?> Handle(ActivateWorkflowVersionCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var activated = await workflowRepository.ActivateVersionAsync(request.Id, request.Version, cancellationToken);
        if (activated is null)
        {
            return null;
        }

        var workflow = JsonSerializer.Deserialize<WorkflowDto>(activated.RuleJson);
        if (workflow is not null)
        {
            rulesEngineWorkflowService.RefreshWorkflow(activated.Id, mapper.Map<Workflow>(workflow));
        }

        return workflow is null
            ? new WorkflowDto
            {
                Id = activated.Id,
                WorkflowName = activated.Name,
                Version = activated.Version,
                IsActive = activated.IsActive,
                EffectiveFromUtc = activated.EffectiveFromUtc,
                EffectiveToUtc = activated.EffectiveToUtc
            }
            : new WorkflowDto
            {
                Id = activated.Id,
                WorkflowName = workflow.WorkflowName,
                RuleExpressionType = workflow.RuleExpressionType,
                GlobalParams = workflow.GlobalParams,
                Rules = workflow.Rules,
                WorkflowsToInject = workflow.WorkflowsToInject,
                Version = activated.Version,
                IsActive = activated.IsActive,
                EffectiveFromUtc = activated.EffectiveFromUtc,
                EffectiveToUtc = activated.EffectiveToUtc
            };
    }
}