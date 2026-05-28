using System.Text.Json.Serialization;

namespace RulesEngine.Application.Dtos;

public sealed class RuleActionsDto
{
    [JsonPropertyName("OnSuccess")]
    public ActionInfoDto? OnSuccess { get; init; }

    [JsonPropertyName("OnFailure")]
    public ActionInfoDto? OnFailure { get; init; }
}
