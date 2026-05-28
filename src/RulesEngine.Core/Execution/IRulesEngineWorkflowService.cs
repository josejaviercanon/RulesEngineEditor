using RulesEngine.Models;

namespace RulesEngine.Core.Execution;

public interface IRulesEngineWorkflowService
{
    /// <summary>
    /// Executes all rules in a workflow against the provided rule parameters.
    /// On the first call for a given <paramref name="workflowId"/>, a <see cref="RulesEngine"/> instance
    /// is created, the workflow is loaded, and JIT compilation runs lazily on the first
    /// <see cref="RulesEngine.ExecuteAllRulesAsync"/> invocation. Subsequent calls reuse the
    /// pre-compiled delegates — zero recompilation cost.
    /// </summary>
    ValueTask<IReadOnlyList<RuleResultTree>> ExecuteWorkflowAsync(
        Guid workflowId,
        Workflow workflowDefinition,
        RuleParameter[] ruleParameters,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the cached engine for a workflow and seeds a fresh instance.
    /// Call this immediately after persisting a create or update to a workflow definition.
    /// The new engine is initialized lazily — JIT compilation happens on the next execute call.
    /// </summary>
    void RefreshWorkflow(Guid workflowId, Workflow workflowDefinition);

    /// <summary>
    /// Removes a workflow's engine from the cache.
    /// Call this immediately after deleting a workflow from persistence.
    /// </summary>
    void EvictWorkflow(Guid workflowId);
}
