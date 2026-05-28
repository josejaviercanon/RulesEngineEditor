namespace RulesEngine.Core.Models;

public sealed class ExecutionStateRecord
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public bool IsDryRun { get; set; }

    public bool WasSuccessful { get; set; }

    public DateTimeOffset ExecutedAtUtc { get; set; }

    public string ResultJson { get; set; } = string.Empty;

    public string? ErrorJson { get; set; }
}
