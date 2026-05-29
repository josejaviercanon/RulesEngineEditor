using System.Text.Json;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

internal static class WorkflowDtoProjection
{
    public static async Task<WorkflowDto> BuildAsync(
        WorkflowRecord record,
        IWorkflowRepository workflowRepository,
        WorkflowRuleQueryMode mode,
        CancellationToken cancellationToken)
    {
        var workflow = JsonSerializer.Deserialize<WorkflowDto>(record.RuleJson);
        var projectedRules = await workflowRepository.ListWorkflowRulesAsync(record.Id, record.Version, mode, cancellationToken);
        var rules = projectedRules.Select(MapRule).ToArray();

        if (workflow is null)
        {
            return new WorkflowDto
            {
                Id = record.Id,
                WorkflowName = record.Name,
                Version = record.Version,
                IsActive = record.IsActive,
                IsEnabled = record.IsEnabled,
                Comments = record.Comments,
                EffectiveFromUtc = record.EffectiveFromUtc,
                EffectiveToUtc = record.EffectiveToUtc,
                Rules = rules
            };
        }

        return new WorkflowDto
        {
            Id = record.Id,
            WorkflowName = workflow.WorkflowName,
            RuleExpressionType = workflow.RuleExpressionType,
            GlobalParams = workflow.GlobalParams,
            Rules = rules,
            WorkflowsToInject = workflow.WorkflowsToInject,
            Version = record.Version,
            IsActive = record.IsActive,
            IsEnabled = record.IsEnabled,
            Comments = record.Comments,
            EffectiveFromUtc = record.EffectiveFromUtc,
            EffectiveToUtc = record.EffectiveToUtc
        };
    }

    public static RuleDto MapRule(RuleVersionRecord record)
    {
        var parsed = JsonSerializer.Deserialize<RuleDto>(record.RuleJson);
        if (parsed is null)
        {
            return new RuleDto
            {
                RuleGuidId = record.RuleGuidId,
                Version = record.Version,
                IsActive = record.IsActive,
                Status = RuleStatusParser.ToValue(record.Status),
                RuleName = record.Name,
                Expression = record.Expression,
                Enabled = true
            };
        }

        return new RuleDto
        {
            RuleGuidId = record.RuleGuidId,
            Version = record.Version,
            IsActive = record.IsActive,
            Status = string.IsNullOrWhiteSpace(parsed.Status)
                ? RuleStatusParser.ToValue(record.Status)
                : parsed.Status,
            RuleName = parsed.RuleName,
            Operator = parsed.Operator,
            ErrorMessage = parsed.ErrorMessage,
            Enabled = parsed.Enabled,
            RuleExpressionType = parsed.RuleExpressionType,
            Expression = parsed.Expression,
            SuccessEvent = parsed.SuccessEvent,
            LocalParams = parsed.LocalParams,
            Rules = parsed.Rules,
            Actions = parsed.Actions,
            WorkflowsToInject = parsed.WorkflowsToInject,
            Properties = parsed.Properties
        };
    }
}
