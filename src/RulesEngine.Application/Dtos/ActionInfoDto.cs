using System.Text.Json;
using System.Text.Json.Serialization;

namespace RulesEngine.Application.Dtos;

public sealed class ActionInfoDto
{
    [JsonPropertyName("Name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("Context")]
    public Dictionary<string, JsonElement> Context { get; init; } = new(StringComparer.Ordinal);
}
