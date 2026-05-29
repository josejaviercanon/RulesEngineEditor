using System.Text.Json;
using System.Text.Json.Nodes;

namespace RulesEngine.Infrastructure.Persistence;

public static class JsonPayloadUtilities
{
    public static string ResolveWorkflowJson(string? workflowJson, string? legacyWorkflowJson)
    {
        if (!string.IsNullOrWhiteSpace(workflowJson))
        {
            return workflowJson;
        }

        if (!string.IsNullOrWhiteSpace(legacyWorkflowJson))
        {
            return legacyWorkflowJson;
        }

        return "{}";
    }

    public static string EnsureRuleJsonContainsExpression(string? rawRuleJson, string expression)
    {
        if (string.IsNullOrWhiteSpace(rawRuleJson))
        {
            return JsonSerializer.Serialize(new { Expression = expression });
        }

        try
        {
            var node = JsonNode.Parse(rawRuleJson);
            if (node is not JsonObject obj)
            {
                return JsonSerializer.Serialize(new { Expression = expression });
            }

            obj["Expression"] = expression;
            return obj.ToJsonString();
        }
        catch (JsonException)
        {
            return JsonSerializer.Serialize(new { Expression = expression });
        }
    }
}
