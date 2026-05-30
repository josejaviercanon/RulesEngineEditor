using RulesEngine.Core.Models;

namespace RulesEngine.Infrastructure.Persistence.Entities;

public sealed class RuleRecord
{
    public Guid Id { get; set; }

    public Guid RuleGuidId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Expression { get; set; } = string.Empty;

    public int ExecuteOrder { get; set; }

    public string RuleJson { get; set; } = string.Empty;

    public int Version { get; set; }

    public bool IsActive { get; set; }

    public RuleStatus Status { get; set; } = RuleStatus.Draft;

    public DateTimeOffset? EffectiveFromUtc { get; set; }

    public DateTimeOffset? EffectiveToUtc { get; set; }
}
