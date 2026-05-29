using RulesEngine.Core.Models;

namespace RulesEngine.Core.Repositories;

public interface IWorkflowRepository
{
    Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(CancellationToken cancellationToken);

    Task<IReadOnlyCollection<WorkflowRecord>> ListVersionsAsync(Guid id, CancellationToken cancellationToken);

    Task<WorkflowRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<WorkflowRecord?> GetVersionAsync(Guid id, int version, CancellationToken cancellationToken);

    Task<WorkflowRecord?> ActivateVersionAsync(Guid id, int version, CancellationToken cancellationToken);

    Task<WorkflowRecord> CreateAsync(WorkflowRecord workflow, CancellationToken cancellationToken);

    Task<WorkflowRecord?> UpdateAsync(Guid id, WorkflowRecord workflow, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

public interface IExecutionStateRepository
{
    Task<Guid> CreateAsync(ExecutionStateRecord record, CancellationToken cancellationToken);
}
