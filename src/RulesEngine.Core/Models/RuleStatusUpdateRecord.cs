namespace RulesEngine.Core.Models;

public sealed class RuleStatusUpdateRecord
{
    public Guid RuleGuidId { get; init; }

    public RuleStatus Status { get; init; }
}
