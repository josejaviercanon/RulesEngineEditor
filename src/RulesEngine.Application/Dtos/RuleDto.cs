using System.Text.Json.Serialization;
using RulesEngine.Models;

namespace RulesEngine.Application.Dtos;

public sealed class RuleDto
{
    [JsonPropertyName("RuleGuidId")]
    public Guid RuleGuidId { get; init; }

    [JsonPropertyName("Version")]
    public int Version { get; init; }

    [JsonPropertyName("ActiveVersion")]
    public int ActiveVersion { get; init; }

    [JsonPropertyName("LastVersion")]
    public int LastVersion { get; init; }

    [JsonPropertyName("IsActive")]
    public bool IsActive { get; init; }

    [JsonPropertyName("Status")]
    public string Status { get; init; } = "draft";

    [JsonPropertyName("RuleName")]
    public string RuleName { get; init; } = string.Empty;

    [JsonPropertyName("Operator")]
    public string? Operator { get; init; }

    [JsonPropertyName("ErrorMessage")]
    public string ErrorMessage { get; init; } = string.Empty;

    [JsonPropertyName("Enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("RuleExpressionType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RuleExpressionType RuleExpressionType { get; init; } = RuleExpressionType.LambdaExpression;

    [JsonPropertyName("Expression")]
    public string Expression { get; init; } = string.Empty;

    [JsonPropertyName("RuleJson")]
    public string RuleJson { get; init; } = string.Empty;

    [JsonPropertyName("SuccessEvent")]
    public string SuccessEvent { get; init; } = string.Empty;

    [JsonPropertyName("LocalParams")]
    public IReadOnlyList<ScopedParamDto> LocalParams { get; init; } = [];

    [JsonPropertyName("Rules")]
    public IReadOnlyList<RuleDto> Rules { get; init; } = [];

    [JsonPropertyName("Actions")]
    public RuleActionsDto? Actions { get; init; }

    [JsonPropertyName("WorkflowsToInject")]
    public IReadOnlyList<string> WorkflowsToInject { get; init; } = [];

    [JsonPropertyName("Properties")]
    public Dictionary<string, string> Properties { get; init; } = new(StringComparer.Ordinal);
}
