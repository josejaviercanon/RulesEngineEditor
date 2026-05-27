namespace RulesEngine.API.Contracts;

public sealed record WorkflowRequest(
    string Name,
    string Expression,
    string RuleJson,
    int Version,
    bool IsActive,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    int? SchemaVersion);

public sealed record WorkflowResponse(
    Guid Id,
    string Name,
    string Expression,
    string RuleJson,
    int Version,
    bool IsActive,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc);

public sealed record ValidationErrorResponse(int SchemaVersion, IReadOnlyCollection<string> Errors);

public sealed record ExecuteWorkflowRequest(bool DryRun, int? SchemaVersion);

public sealed record ExecuteWorkflowResponse(
    bool DryRun,
    int SchemaVersion,
    bool Persisted,
    bool WasSuccessful,
    string ResultJson,
    Guid? ExecutionId);

public sealed record ExecutionErrorResponse(string Code, string Message, int? SchemaVersion, IReadOnlyCollection<string>? Errors);
