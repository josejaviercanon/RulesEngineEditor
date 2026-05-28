using RulesEngine.Core.Repositories;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Infrastructure.Persistence.Entities;
using CoreExecutionStateRecord = RulesEngine.Core.Models.ExecutionStateRecord;
using PersistenceExecutionStateRecord = RulesEngine.Infrastructure.Persistence.Entities.ExecutionStateRecord;

namespace RulesEngine.Infrastructure.Repositories;

public sealed class ExecutionStateRepository(RulesEngineEditorDbContext dbContext) : IExecutionStateRepository
{
    public async Task<Guid> CreateAsync(CoreExecutionStateRecord record, CancellationToken cancellationToken)
    {
        var entity = new PersistenceExecutionStateRecord
        {
            Id = record.Id == Guid.Empty ? Guid.NewGuid() : record.Id,
            WorkflowId = record.WorkflowId,
            IsDryRun = record.IsDryRun,
            WasSuccessful = record.WasSuccessful,
            ExecutedAtUtc = record.ExecutedAtUtc,
            ResultJson = record.ResultJson,
            ErrorJson = record.ErrorJson
        };

        dbContext.ExecutionStates.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return entity.Id;
    }
}