using RulesEngine.Core.Models;

namespace RulesEngine.Application.Policies;

public sealed class RuleStatusPolicy : IRuleStatusPolicy
{
    public RuleStatus NormalizeOrDefault(string? value, RuleStatus fallback = RuleStatus.Draft)
        => RuleStatusParser.ParseOrDefault(value, fallback);

    public bool IsValidStatus(string? value)
        => RuleStatusParser.TryParse(value, out _);

    public RuleStatus ResolveUserRequestedStatus(
        RuleStatus? previousStatus,
        string? requestedStatus,
        bool validationsPassed)
    {
        var fallback = previousStatus ?? RuleStatus.Draft;
        var normalized = NormalizeOrDefault(requestedStatus, fallback);

        if (normalized == RuleStatus.Production && !validationsPassed)
        {
            return fallback;
        }

        return normalized;
    }

    public RuleStatus ResolveCompileFailureStatus(RuleStatus currentStatus)
        => currentStatus switch
        {
            RuleStatus.Draft => RuleStatus.Draft,
            RuleStatus.Disabled => RuleStatus.Disabled,
            RuleStatus.Failed => RuleStatus.Failed,
            _ => RuleStatus.Failed
        };

    public RuleStatus ResolveExecutionFailureStatus(RuleStatus currentStatus)
        => currentStatus switch
        {
            RuleStatus.Production => RuleStatus.Failed,
            RuleStatus.Draft => RuleStatus.Draft,
            RuleStatus.Failed => RuleStatus.Failed,
            RuleStatus.Disabled => RuleStatus.Disabled,
            _ => RuleStatus.Failed
        };
}
