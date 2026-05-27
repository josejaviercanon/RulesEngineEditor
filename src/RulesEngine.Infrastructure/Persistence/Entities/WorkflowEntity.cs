namespace RulesEngine.Infrastructure.Persistence.Entities;

public sealed class WorkflowEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public int Version { get; set; }

    public bool IsActive { get; set; }

    public DateTimeOffset? EffectiveFromUtc { get; set; }

    public DateTimeOffset? EffectiveToUtc { get; set; }

    public WorkflowDefinitionEntity Definition { get; set; } = new();
}

public sealed class WorkflowDefinitionEntity
{
    public string Expression { get; set; } = string.Empty;

    public string RuleJson { get; set; } = string.Empty;
}
