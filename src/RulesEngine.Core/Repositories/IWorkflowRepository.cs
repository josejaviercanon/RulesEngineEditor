using RulesEngine.Core.Models;

namespace RulesEngine.Core.Repositories;

public interface IWorkflowRepository
{
    Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(
        CancellationToken cancellationToken,
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly);

    Task<IReadOnlyCollection<WorkflowRecord>> ListVersionsAsync(
        Guid id,
        CancellationToken cancellationToken,
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly);

    Task<WorkflowRecord?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken,
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly);

    Task<WorkflowRecord?> GetVersionAsync(
        Guid id,
        int version,
        CancellationToken cancellationToken,
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly);

    Task<WorkflowRecord?> ActivateVersionAsync(Guid id, int version, CancellationToken cancellationToken);

    Task<WorkflowRecord> CreateAsync(WorkflowRecord workflow, CancellationToken cancellationToken);

    Task<WorkflowRecord?> UpdateAsync(Guid id, WorkflowRecord workflow, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RuleVersionRecord>> ListRuleVersionsAsync(
        Guid workflowId,
        Guid ruleGuidId,
        CancellationToken cancellationToken);

    Task<RuleVersionRecord?> ActivateRuleVersionAsync(
        Guid workflowId,
        Guid ruleGuidId,
        int version,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<RuleVersionRecord>> ListWorkflowRulesAsync(
        Guid workflowId,
        int workflowVersion,
        WorkflowRuleQueryMode mode,
        CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IExecutionStateRepository
{
    Task<Guid> CreateAsync(ExecutionStateRecord record, CancellationToken cancellationToken);
}
