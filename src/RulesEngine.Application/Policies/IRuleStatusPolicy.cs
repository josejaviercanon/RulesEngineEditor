using RulesEngine.Core.Models;

namespace RulesEngine.Application.Policies;

public interface IRuleStatusPolicy
{
    RuleStatus NormalizeOrDefault(string? value, RuleStatus fallback = RuleStatus.Draft);

    bool IsValidStatus(string? value);

    RuleStatus ResolveUserRequestedStatus(
        RuleStatus? previousStatus,
        string? requestedStatus,
        bool validationsPassed);

    RuleStatus ResolveCompileFailureStatus(RuleStatus currentStatus);

    RuleStatus ResolveExecutionFailureStatus(RuleStatus currentStatus);
}
