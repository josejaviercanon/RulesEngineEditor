using System.Text.Json;
using AutoMapper;
using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Application.Exceptions;
using RulesEngine.Application.Policies;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;
using RulesEngine.Exceptions;
using RulesEngine.Models;

namespace RulesEngine.Application.Handlers;

public sealed class CreateWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IRulesEngineWorkflowService rulesEngineWorkflowService,
    IMapper mapper,
    IRuleStatusPolicy ruleStatusPolicy)
    : IRequestHandler<CreateWorkflowCommand, WorkflowDto>
{
    public async Task<WorkflowDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateWorkflowMetadata(request.Workflow);

        var normalized = NormalizeRuleStatuses(request.Workflow, previousStatuses: null);
        var workflowDefinition = mapper.Map<Workflow>(normalized);
        var validationsPassed = TryValidateWorkflow(workflowDefinition, out var validationErrors);

        if (!validationsPassed)
        {
            throw new WorkflowValidationException(validationErrors);
        }

        normalized = AddJsonPayloads(normalized);

        var record = new WorkflowRecord
        {
            Id = request.Workflow.Id == Guid.Empty ? Guid.NewGuid() : request.Workflow.Id,
            Name = normalized.WorkflowName,
            Expression = string.Empty,
            WorkflowJson = normalized.WorkflowJson,
            RuleJson = normalized.WorkflowJson,
            IsEnabled = normalized.IsEnabled,
            Comments = normalized.Comments,
            EffectiveFromUtc = normalized.EffectiveFromUtc,
            EffectiveToUtc = normalized.EffectiveToUtc
        };

        var created = await workflowRepository.CreateAsync(record, cancellationToken);

        rulesEngineWorkflowService.RefreshWorkflow(created.Id, workflowDefinition);

        return await WorkflowDtoProjection.BuildAsync(
            created,
            workflowRepository,
            WorkflowRuleQueryMode.ActiveOnly,
            cancellationToken);
    }

    private static bool TryValidateWorkflow(Workflow workflow, out IReadOnlyList<string> errors)
    {
        try
        {
            var engine = new global::RulesEngine.RulesEngine();
            engine.AddOrUpdateWorkflow(workflow);
            errors = [];
            return true;
        }
        catch (RuleValidationException ex)
        {
            errors = ex.Errors.Select(error => error.ErrorMessage).ToArray();
            return false;
        }
    }

    private WorkflowDto NormalizeRuleStatuses(
        WorkflowDto workflow,
        IReadOnlyDictionary<Guid, RuleStatus>? previousStatuses)
    {
        var normalizedRules = workflow.Rules
            .Select(rule =>
            {
                if (!string.IsNullOrWhiteSpace(rule.Status) && !ruleStatusPolicy.IsValidStatus(rule.Status))
                {
                    throw new InvalidOperationException($"Invalid status '{rule.Status}' for rule '{rule.RuleName}'.");
                }

                var previous = rule.RuleGuidId != Guid.Empty && previousStatuses is not null && previousStatuses.TryGetValue(rule.RuleGuidId, out var previousStatus)
                    ? (RuleStatus?)previousStatus
                    : null;

                var requested = ruleStatusPolicy.ResolveUserRequestedStatus(previous, rule.Status, validationsPassed: true);
                var final = requested;

                return new RuleDto
                {
                    RuleGuidId = rule.RuleGuidId,
                    Version = rule.Version,
                    IsActive = rule.IsActive,
                    Status = RuleStatusParser.ToValue(final),
                    RuleName = rule.RuleName,
                    Operator = rule.Operator,
                    ErrorMessage = rule.ErrorMessage,
                    Enabled = rule.Enabled,
                    RuleExpressionType = rule.RuleExpressionType,
                    Expression = rule.Expression,
                    ExecuteOrder = rule.ExecuteOrder,
                    SuccessEvent = rule.SuccessEvent,
                    LocalParams = rule.LocalParams,
                    Rules = rule.Rules,
                    Actions = rule.Actions,
                    WorkflowsToInject = rule.WorkflowsToInject,
                    Properties = rule.Properties
                };
            })
            .ToArray();

        return new WorkflowDto
        {
            Id = workflow.Id,
            WorkflowName = workflow.WorkflowName,
            RuleExpressionType = workflow.RuleExpressionType,
            GlobalParams = workflow.GlobalParams,
            Rules = normalizedRules,
            WorkflowsToInject = workflow.WorkflowsToInject,
            WorkflowJson = workflow.WorkflowJson,
            Version = workflow.Version,
            IsActive = workflow.IsActive,
            IsEnabled = workflow.IsEnabled,
            Comments = workflow.Comments,
            EffectiveFromUtc = workflow.EffectiveFromUtc,
            EffectiveToUtc = workflow.EffectiveToUtc
        };
    }

    private WorkflowDto AddJsonPayloads(WorkflowDto workflow)
    {
        var normalizedRules = workflow.Rules
            .Select(rule =>
            {
                var serializedRule = BuildRuleJsonPayload(rule);

                return new RuleDto
                {
                    RuleGuidId = rule.RuleGuidId,
                    Version = rule.Version,
                    IsActive = rule.IsActive,
                    Status = rule.Status,
                    RuleName = rule.RuleName,
                    Operator = rule.Operator,
                    ErrorMessage = rule.ErrorMessage,
                    Enabled = rule.Enabled,
                    RuleExpressionType = rule.RuleExpressionType,
                    Expression = rule.Expression,
                    ExecuteOrder = rule.ExecuteOrder,
                    RuleJson = serializedRule,
                    SuccessEvent = rule.SuccessEvent,
                    LocalParams = rule.LocalParams,
                    Rules = rule.Rules,
                    Actions = rule.Actions,
                    WorkflowsToInject = rule.WorkflowsToInject,
                    Properties = rule.Properties
                };
            })
            .ToArray();

        return new WorkflowDto
        {
            Id = workflow.Id,
            WorkflowName = workflow.WorkflowName,
            RuleExpressionType = workflow.RuleExpressionType,
            GlobalParams = workflow.GlobalParams,
            Rules = normalizedRules,
            WorkflowsToInject = workflow.WorkflowsToInject,
            WorkflowJson = BuildWorkflowJsonPayload(workflow, normalizedRules),
            Version = workflow.Version,
            IsActive = workflow.IsActive,
            IsEnabled = workflow.IsEnabled,
            Comments = workflow.Comments,
            EffectiveFromUtc = workflow.EffectiveFromUtc,
            EffectiveToUtc = workflow.EffectiveToUtc
        };
    }

    private static string BuildWorkflowJsonPayload(WorkflowDto workflow, IReadOnlyCollection<RuleDto> rules)
        => JsonSerializer.Serialize(new
        {
            workflow.WorkflowName,
            workflow.RuleExpressionType,
            workflow.GlobalParams,
            Rules = rules.Select(rule => new
            {
                rule.RuleGuidId,
                rule.Version,
                rule.IsActive,
                rule.Status,
                rule.RuleName,
                rule.Operator,
                rule.ErrorMessage,
                rule.Enabled,
                rule.RuleExpressionType,
                rule.Expression,
                rule.ExecuteOrder,
                rule.SuccessEvent,
                rule.LocalParams,
                rule.Rules,
                rule.Actions,
                rule.WorkflowsToInject,
                rule.Properties
            }),
            workflow.WorkflowsToInject
        });

    private static string BuildRuleJsonPayload(RuleDto rule)
        => JsonSerializer.Serialize(new
        {
            rule.RuleGuidId,
            rule.Version,
            rule.IsActive,
            rule.Status,
            rule.RuleName,
            rule.Operator,
            rule.ErrorMessage,
            rule.Enabled,
            rule.RuleExpressionType,
            rule.Expression,
            rule.ExecuteOrder,
            rule.SuccessEvent,
            rule.LocalParams,
            rule.Rules,
            rule.Actions,
            rule.WorkflowsToInject,
            rule.Properties
        });

    private static void ValidateWorkflowMetadata(WorkflowDto workflow)
    {
        if (workflow.Comments is not null && workflow.Comments.Length > 4000)
        {
            throw new InvalidOperationException("Comments cannot exceed 4000 characters.");
        }
    }
}
