namespace RulesEngine.Core.Models;

public sealed class RuleVersionRecord
{
    public Guid Id { get; set; }

    public Guid RuleGuidId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Expression { get; set; } = string.Empty;

    public string RuleJson { get; set; } = string.Empty;

    public int Version { get; set; }

    public int ActiveVersion { get; set; }

    public int LastVersion { get; set; }

    public bool IsActive { get; set; }

    public RuleStatus Status { get; set; } = RuleStatus.Draft;

    public DateTimeOffset? EffectiveFromUtc { get; set; }

    public DateTimeOffset? EffectiveToUtc { get; set; }
}
