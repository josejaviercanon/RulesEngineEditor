using System.Text.Json;
using RulesEngine.Models;

namespace RulesEngine.Core.Execution;

public sealed class RulesEngineWorkflowService : IRulesEngineWorkflowService
{
    private readonly global::RulesEngine.RulesEngine _rulesEngine = new();

    public void AddOrUpdateWorkflow(string workflowJson)
    {
        if (string.IsNullOrWhiteSpace(workflowJson))
        {
            throw new ArgumentException("Workflow JSON is required.", nameof(workflowJson));
        }

        var workflow = JsonSerializer.Deserialize<Workflow>(workflowJson);
        if (workflow is null)
        {
            throw new InvalidOperationException("Unable to deserialize workflow JSON.");
        }

        _rulesEngine.AddOrUpdateWorkflow(workflow);
    }

    public ValueTask<List<RuleResultTree>> ExecuteAllRulesAsync(string workflowName, params RuleParameter[] ruleParameters)
    {
        return _rulesEngine.ExecuteAllRulesAsync(workflowName, ruleParameters);
    }
}
