namespace RulesEngine.Infrastructure.Persistence.Entities;

public sealed class WorkflowRuleCollectionEntity
{
    public Guid WorkflowId { get; set; }

    public int WorkflowVersion { get; set; }

    public Guid RuleGuidId { get; set; }

    public int RuleVersion { get; set; }
}
