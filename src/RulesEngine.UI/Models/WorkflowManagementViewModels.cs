namespace RulesEngine.UI.Models;

public sealed record WorkflowGridRow(
    Guid Id,
    int LastVersion,
    int ActiveVersion,
    string WorkflowName,
    bool IsEnabled,
    bool IsActive);

public sealed record WorkflowVersionItem(
    int Version,
    bool IsActive,
    bool IsEnabled,
    string WorkflowName,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc,
    string? Comments);

public sealed record RuleGridRow(
    Guid RuleGuidId,
    int Version,
    int ActiveVersion,
    int LastVersion,
    string RuleName,
    int ExecuteOrder,
    string Expression,
    string Status,
    bool IsActive);
