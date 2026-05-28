namespace RulesEngine.Application.Dtos;

public sealed class WorkflowDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string Expression { get; init; } = string.Empty;

    public string RuleJson { get; init; } = string.Empty;

    public int Version { get; init; }

    public bool IsActive { get; init; }

    public DateTimeOffset? EffectiveFromUtc { get; init; }

    public DateTimeOffset? EffectiveToUtc { get; init; }
}
