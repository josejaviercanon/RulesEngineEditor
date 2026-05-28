using System.Collections.Concurrent;
using RulesEngine.Models;

namespace RulesEngine.Core.Execution;

/// <summary>
/// Singleton service that maintains a per-workflow cache of pre-compiled <see cref="RulesEngine"/> instances.
///
/// WHY A SINGLETON WITH PER-WORKFLOW ENGINE INSTANCES:
///   Dynamic LINQ expression compilation (System.Linq.Expressions.Expression.Compile) is CPU-intensive
///   and allocates compiled delegates on the Large Object Heap. Creating a new engine per request
///   causes repeated JIT compilation of identical expressions, Gen 2 GC pressure, and OOM risk under load.
///   By caching one engine per workflow, compilation runs exactly once per workflow version.
///
/// THREAD SAFETY:
///   ConcurrentDictionary provides safe concurrent read/write on the cache entries.
///   Lazy&lt;T&gt; with LazyThreadSafetyMode.ExecutionAndPublication guarantees that only one thread
///   performs JIT compilation per workflow — all concurrent callers block on .Value until
///   compilation completes, then share the result with zero recompilation cost.
///
/// CACHE INVALIDATION:
///   Call RefreshWorkflow() after persisting a create or update.
///   Call EvictWorkflow() after persisting a delete.
///   The cache is in-process only — see follow-up notes for distributed invalidation.
/// </summary>
public sealed class RulesEngineWorkflowService : IRulesEngineWorkflowService
{
    private readonly ConcurrentDictionary<Guid, Lazy<global::RulesEngine.RulesEngine>> _engines = new();

    /// <inheritdoc/>
    public async ValueTask<IReadOnlyList<RuleResultTree>> ExecuteWorkflowAsync(
        Guid workflowId,
        Workflow workflowDefinition,
        RuleParameter[] ruleParameters,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var engine = GetOrCreateEngine(workflowId, workflowDefinition);

        // ExecuteAllRulesAsync is thread-safe. On the first call, RegisterRule lazily
        // JIT-compiles the expressions and caches the delegates inside the engine's
        // internal RulesCache. Subsequent calls skip compilation entirely.
        var results = await engine.ExecuteAllRulesAsync(workflowDefinition.WorkflowName, ruleParameters);
        return results.AsReadOnly();
    }

    /// <inheritdoc/>
    public void RefreshWorkflow(Guid workflowId, Workflow workflowDefinition)
    {
        // Replace atomically. The old engine (and its compiled delegates) will be collected
        // by GC once no in-flight requests hold a reference to it.
        var replacement = BuildLazy(workflowDefinition);
        _engines.AddOrUpdate(workflowId, replacement, (_, _) => replacement);
    }

    /// <inheritdoc/>
    public void EvictWorkflow(Guid workflowId)
    {
        _engines.TryRemove(workflowId, out _);
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private global::RulesEngine.RulesEngine GetOrCreateEngine(Guid workflowId, Workflow workflowDefinition)
    {
        // GetOrAdd is atomic on the key level. Multiple threads may concurrently construct
        // Lazy<T> instances, but only one wins the race and is stored. The Lazy<T> itself
        // ensures single execution of the factory via ExecutionAndPublication mode.
        return _engines.GetOrAdd(workflowId, _ => BuildLazy(workflowDefinition)).Value;
    }

    private static Lazy<global::RulesEngine.RulesEngine> BuildLazy(Workflow workflowDefinition)
    {
        return new Lazy<global::RulesEngine.RulesEngine>(
            () => CreateEngine(workflowDefinition),
            LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private static global::RulesEngine.RulesEngine CreateEngine(Workflow workflowDefinition)
    {
        var engine = new global::RulesEngine.RulesEngine(reSettings: new ReSettings
        {
            // Surface expression parse/compile errors as RuleResultTree.ExceptionMessage
            // rather than throwing exceptions. This lets callers inspect results uniformly
            // without try/catch around ExecuteAllRulesAsync.
            EnableExceptionAsErrorMessage = true,
            EnableExceptionAsErrorMessageForRuleExpressionParsing = true
        });

        // AddOrUpdateWorkflow runs WorkflowsValidator (internal FluentValidation) and throws
        // RuleValidationException for structural errors (empty names, missing expressions, etc.).
        // Expression compilation does NOT happen here — it's deferred to ExecuteAllRulesAsync.
        engine.AddOrUpdateWorkflow(workflowDefinition);
        return engine;
    }
}
