namespace RulesEngine.Application.Dtos;

public sealed class RuleStatusTransitionDto
{
    public Guid RuleGuidId { get; init; }

    public string RuleName { get; init; } = string.Empty;

    public string StatusBefore { get; init; } = "draft";

    public string StatusAfter { get; init; } = "draft";

    public string? TransitionReason { get; init; }
}
