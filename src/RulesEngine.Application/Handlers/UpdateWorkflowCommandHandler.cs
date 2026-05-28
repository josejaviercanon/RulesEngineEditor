using System.Text.Json;
using AutoMapper;
using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;
using RulesEngine.Exceptions;
using RulesEngine.Models;

namespace RulesEngine.Application.Handlers;

public sealed class UpdateWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IRulesEngineWorkflowService rulesEngineWorkflowService,
    IMapper mapper)
    : IRequestHandler<UpdateWorkflowCommand, WorkflowDto?>
{
    public async Task<WorkflowDto?> Handle(UpdateWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workflowDefinition = mapper.Map<Workflow>(request.Workflow);
        EnsureWorkflowIsStructurallyValid(workflowDefinition);

        var updated = await workflowRepository.UpdateAsync(request.Id, new WorkflowRecord
        {
            Name = request.Workflow.WorkflowName,
            Expression = string.Empty,
            RuleJson = JsonSerializer.Serialize(request.Workflow),
            Version = request.Workflow.Version,
            IsActive = request.Workflow.IsActive,
            EffectiveFromUtc = request.Workflow.EffectiveFromUtc,
            EffectiveToUtc = request.Workflow.EffectiveToUtc
        }, cancellationToken);

        if (updated is null)
        {
            return null;
        }

        rulesEngineWorkflowService.RefreshWorkflow(updated.Id, workflowDefinition);

        return new WorkflowDto
        {
            Id = updated.Id,
            WorkflowName = request.Workflow.WorkflowName,
            RuleExpressionType = request.Workflow.RuleExpressionType,
            GlobalParams = request.Workflow.GlobalParams,
            Rules = request.Workflow.Rules,
            WorkflowsToInject = request.Workflow.WorkflowsToInject,
            Version = request.Workflow.Version,
            IsActive = request.Workflow.IsActive,
            EffectiveFromUtc = request.Workflow.EffectiveFromUtc,
            EffectiveToUtc = request.Workflow.EffectiveToUtc
        };
    }

    private static void EnsureWorkflowIsStructurallyValid(Workflow workflow)
    {
        try
        {
            var engine = new global::RulesEngine.RulesEngine();
            engine.AddOrUpdateWorkflow(workflow);
        }
        catch (RuleValidationException ex)
        {
            throw new InvalidOperationException(
                string.Join("; ", ex.Errors.Select(error => error.ErrorMessage)),
                ex);
        }
    }
}
