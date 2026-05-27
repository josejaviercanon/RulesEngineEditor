using RulesEngine.Models;

namespace RulesEngine.Core.Execution;

public interface IRulesEngineWorkflowService
{
    void AddOrUpdateWorkflow(string workflowJson);

    ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params RuleParameter[] ruleParameters);
}
