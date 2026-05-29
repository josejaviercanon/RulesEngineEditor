namespace RulesEngine.Application.Dtos;

public sealed class ExecuteWorkflowResultDto
{
    public bool Found { get; init; } = true;

    public bool IsSuccess { get; init; }

    public bool DryRun { get; init; }

    public bool Persisted { get; init; }

    public bool WasSuccessful { get; init; }

    public IReadOnlyList<RuleResultDto> Results { get; init; } = [];

    public IReadOnlyList<RuleStatusTransitionDto> RuleStatusTransitions { get; init; } = [];

    public Guid? ExecutionId { get; init; }

    public int? SchemaVersion { get; init; }

    public string? ErrorCode { get; init; }

    public string? ErrorMessage { get; init; }

    public IReadOnlyCollection<string>? Errors { get; init; }
}
