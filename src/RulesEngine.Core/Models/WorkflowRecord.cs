namespace RulesEngine.Core.Models;

public sealed class WorkflowRecord
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Expression { get; set; } = string.Empty;

    public string RuleJson { get; set; } = string.Empty;

    public int Version { get; set; }

    public bool IsActive { get; set; }

    public bool IsEnabled { get; set; } = true;

    public string? Comments { get; set; }

    public DateTimeOffset? EffectiveFromUtc { get; set; }

    public DateTimeOffset? EffectiveToUtc { get; set; }

    public WorkflowRuleQueryMode RuleQueryMode { get; set; } = WorkflowRuleQueryMode.ActiveOnly;
}
