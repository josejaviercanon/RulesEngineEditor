namespace RulesEngine.Application.Dtos;

public sealed class RuleResultDto
{
    public string RuleName { get; init; } = string.Empty;

    public bool IsSuccess { get; init; }

    public string? ExceptionMessage { get; init; }

    public string? SuccessEvent { get; init; }

    public string? ActionOutput { get; init; }

    public IReadOnlyList<RuleResultDto> ChildResults { get; init; } = [];
}
