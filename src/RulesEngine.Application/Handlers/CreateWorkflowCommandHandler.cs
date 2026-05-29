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

public sealed class CreateWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IRulesEngineWorkflowService rulesEngineWorkflowService,
    IMapper mapper)
    : IRequestHandler<CreateWorkflowCommand, WorkflowDto>
{
    public async Task<WorkflowDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workflowDefinition = mapper.Map<Workflow>(request.Workflow);
        EnsureWorkflowIsStructurallyValid(workflowDefinition);

        var record = new WorkflowRecord
        {
            Id = request.Workflow.Id == Guid.Empty ? Guid.NewGuid() : request.Workflow.Id,
            Name = request.Workflow.WorkflowName,
            Expression = string.Empty,
            RuleJson = JsonSerializer.Serialize(request.Workflow),
            EffectiveFromUtc = request.Workflow.EffectiveFromUtc,
            EffectiveToUtc = request.Workflow.EffectiveToUtc
        };

        var created = await workflowRepository.CreateAsync(record, cancellationToken);

        rulesEngineWorkflowService.RefreshWorkflow(created.Id, workflowDefinition);

        return new WorkflowDto
        {
            Id = created.Id,
            WorkflowName = request.Workflow.WorkflowName,
            RuleExpressionType = request.Workflow.RuleExpressionType,
            GlobalParams = request.Workflow.GlobalParams,
            Rules = request.Workflow.Rules,
            WorkflowsToInject = request.Workflow.WorkflowsToInject,
            Version = created.Version,
            IsActive = created.IsActive,
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
