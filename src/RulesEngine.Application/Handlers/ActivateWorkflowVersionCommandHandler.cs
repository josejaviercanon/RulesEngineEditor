using System.Text.Json;
using AutoMapper;
using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Models;
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

        return await WorkflowDtoProjection.BuildAsync(
            activated,
            workflowRepository,
            WorkflowRuleQueryMode.ActiveOnly,
            cancellationToken);
    }
}