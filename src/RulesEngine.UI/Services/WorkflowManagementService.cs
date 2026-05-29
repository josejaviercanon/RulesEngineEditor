using RulesEngine.UI.Models;

namespace RulesEngine.UI.Services;

public sealed class WorkflowManagementService(WorkflowApiClient apiClient)
{
    public async Task<IReadOnlyList<WorkflowGridRow>> ListWorkflowGridRowsAsync(CancellationToken cancellationToken = default)
    {
        var workflows = await apiClient.ListWorkflowsAsync(cancellationToken);

        return workflows
            .Select(workflow => new WorkflowGridRow(
                workflow.Id,
                workflow.LastVersion,
                workflow.ActiveVersion,
                workflow.Workflow.WorkflowName,
                workflow.IsEnabled,
                workflow.IsActive))
            .OrderBy(row => row.WorkflowName)
            .ThenBy(row => row.Id)
            .ToArray();
    }

    public async Task<IReadOnlyList<WorkflowVersionItem>> ListWorkflowVersionsAsync(Guid workflowId, CancellationToken cancellationToken = default)
    {
        var versions = await apiClient.ListWorkflowVersionsAsync(workflowId, cancellationToken);

        return versions
            .OrderBy(item => item.Version)
            .Select(item => new WorkflowVersionItem(
                item.Version,
                item.IsActive,
                item.IsEnabled,
                item.Workflow.WorkflowName,
                item.Workflow.EffectiveFromUtc,
                item.Workflow.EffectiveToUtc,
                item.Workflow.Comments))
            .ToArray();
    }

    public static IReadOnlyList<RuleGridRow> ToRuleGridRows(WorkflowPayload workflow)
        => workflow.Rules
            .Select(rule => new RuleGridRow(
                rule.RuleGuidId,
                rule.Version,
                rule.RuleName,
                rule.Expression,
                rule.Status,
                rule.IsActive))
            .OrderBy(rule => rule.RuleName)
            .ThenBy(rule => rule.RuleGuidId)
            .ThenBy(rule => rule.Version)
            .ToArray();
}
