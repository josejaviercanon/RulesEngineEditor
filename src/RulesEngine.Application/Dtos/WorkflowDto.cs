using System.Text.Json.Serialization;
using RulesEngine.Models;

namespace RulesEngine.Application.Dtos;

public sealed class WorkflowDto
{
    public Guid Id { get; init; }

    [JsonPropertyName("WorkflowName")]
    public string WorkflowName { get; init; } = string.Empty;

    [JsonPropertyName("RuleExpressionType")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RuleExpressionType RuleExpressionType { get; init; } = RuleExpressionType.LambdaExpression;

    [JsonPropertyName("GlobalParams")]
    public IReadOnlyList<ScopedParamDto> GlobalParams { get; init; } = [];

    [JsonPropertyName("Rules")]
    public IReadOnlyList<RuleDto> Rules { get; init; } = [];

    [JsonPropertyName("WorkflowsToInject")]
    public IReadOnlyList<string> WorkflowsToInject { get; init; } = [];

    public string WorkflowJson { get; init; } = string.Empty;

    public int Version { get; init; }

    public int ActiveVersion { get; init; }

    public int LastVersion { get; init; }

    public bool IsActive { get; init; }

    public bool IsEnabled { get; init; } = true;

    public string? Comments { get; init; }

    public DateTimeOffset? EffectiveFromUtc { get; init; }

    public DateTimeOffset? EffectiveToUtc { get; init; }

    public IReadOnlyList<string> ValidationWarnings { get; init; } = [];
}
