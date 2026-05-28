using System.Text.Json.Serialization;

namespace RulesEngine.Application.Dtos;

public sealed class ScopedParamDto
{
    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("Expression")]
    public string Expression { get; init; } = string.Empty;
}
