namespace RulesEngine.Application.Dtos;

public sealed class RuleResultDto
{
    public Guid RuleGuidId { get; init; }

    public string RuleName { get; init; } = string.Empty;

    public bool IsSuccess { get; init; }

    public string? ExceptionMessage { get; init; }

    public string? SuccessEvent { get; init; }

    public string? ActionOutput { get; init; }

    public string? StatusBefore { get; init; }

    public string? StatusAfter { get; init; }

    public string? TransitionReason { get; init; }

    public IReadOnlyList<RuleResultDto> ChildResults { get; init; } = [];
}
