using Microsoft.EntityFrameworkCore;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Infrastructure.Persistence.Entities;

namespace RulesEngine.Infrastructure.Repositories;

public sealed class WorkflowRepository(RulesEngineEditorDbContext dbContext) : IWorkflowRepository
{
    public async Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(CancellationToken cancellationToken)
    {
        var records = await LoadRecordsAsync(cancellationToken);
        return records
            .GroupBy(record => record.Id)
            .Select(group => group
                .OrderByDescending(record => record.IsActive)
                .ThenByDescending(record => record.Version)
                .First())
            .OrderBy(record => record.Name)
            .ThenBy(record => record.Id)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<WorkflowRecord>> ListVersionsAsync(Guid id, CancellationToken cancellationToken)
        => (await LoadRecordsAsync(cancellationToken, id))
            .OrderBy(record => record.Version)
            .ToArray();

    public async Task<WorkflowRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var records = await LoadRecordsAsync(cancellationToken, id);
        return records
            .OrderByDescending(record => record.IsActive)
            .ThenByDescending(record => record.Version)
            .FirstOrDefault();
    }

    public async Task<WorkflowRecord?> GetVersionAsync(Guid id, int version, CancellationToken cancellationToken)
        => (await LoadRecordsAsync(cancellationToken, id))
            .FirstOrDefault(record => record.Version == version);

    public async Task<WorkflowRecord?> ActivateVersionAsync(Guid id, int version, CancellationToken cancellationToken)
    {
        var entities = await dbContext.Workflows
            .Where(workflow => workflow.Id == id)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            return null;
        }

        var target = entities.FirstOrDefault(entity => entity.Version == version);
        if (target is null)
        {
            return null;
        }

        foreach (var entity in entities)
        {
            entity.IsActive = entity.Version == version;
        }

        await SaveChangesAsync(cancellationToken);
        return MapToCore(target);
    }

    public async Task<WorkflowRecord> CreateAsync(WorkflowRecord workflow, CancellationToken cancellationToken)
    {
        var workflowId = workflow.Id == Guid.Empty ? Guid.NewGuid() : workflow.Id;
        var nextVersion = await GetNextVersionAsync(workflowId, cancellationToken);

        await DeactivateCurrentActiveAsync(workflowId, cancellationToken);

        var entity = new WorkflowEntity
        {
            Id = workflowId,
            Name = workflow.Name,
            Version = nextVersion,
            IsActive = true,
            EffectiveFromUtc = workflow.EffectiveFromUtc,
            EffectiveToUtc = workflow.EffectiveToUtc,
            Definition = new WorkflowDefinitionEntity
            {
                Expression = workflow.Expression,
                RuleJson = workflow.RuleJson
            }
        };

        dbContext.Workflows.Add(entity);
        await SaveChangesAsync(cancellationToken);

        return MapToCore(entity);
    }

    public async Task<WorkflowRecord?> UpdateAsync(Guid id, WorkflowRecord workflow, CancellationToken cancellationToken)
    {
        var existing = await dbContext.Workflows
            .Where(current => current.Id == id)
            .ToListAsync(cancellationToken);

        if (existing.Count == 0)
        {
            return null;
        }

        var nextVersion = existing.Max(entity => entity.Version) + 1;

        foreach (var entity in existing.Where(entity => entity.IsActive))
        {
            entity.IsActive = false;
        }

        var entityToAdd = new WorkflowEntity
        {
            Id = id,
            Name = workflow.Name,
            Version = nextVersion,
            IsActive = true,
            EffectiveFromUtc = workflow.EffectiveFromUtc,
            EffectiveToUtc = workflow.EffectiveToUtc,
            Definition = new WorkflowDefinitionEntity
            {
                Expression = workflow.Expression,
                RuleJson = workflow.RuleJson
            }
        };

        dbContext.Workflows.Add(entityToAdd);

        await SaveChangesAsync(cancellationToken);

        return MapToCore(entityToAdd);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entities = await dbContext.Workflows
            .Where(workflow => workflow.Id == id)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            return false;
        }

        dbContext.Workflows.RemoveRange(entities);
        await SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        if (dbContext.Database.IsRelational())
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task DeactivateCurrentActiveAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        var activeEntities = await dbContext.Workflows
            .Where(workflow => workflow.Id == workflowId && workflow.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var entity in activeEntities)
        {
            entity.IsActive = false;
        }
    }

    private async Task<int> GetNextVersionAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        var latestVersion = await dbContext.Workflows
            .Where(workflow => workflow.Id == workflowId)
            .Select(workflow => (int?)workflow.Version)
            .MaxAsync(cancellationToken);

        return (latestVersion ?? 0) + 1;
    }

    private async Task<List<WorkflowRecord>> LoadRecordsAsync(CancellationToken cancellationToken, Guid? workflowId = null)
    {
        var query = dbContext.Workflows.AsNoTracking().AsQueryable();
        if (workflowId.HasValue)
        {
            query = query.Where(workflow => workflow.Id == workflowId.Value);
        }

        var entities = await query.ToListAsync(cancellationToken);
        return entities.Select(MapToCore).ToList();
    }

    private static WorkflowRecord MapToCore(WorkflowEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Expression = entity.Definition.Expression,
        RuleJson = entity.Definition.RuleJson,
        Version = entity.Version,
        IsActive = entity.IsActive,
        EffectiveFromUtc = entity.EffectiveFromUtc,
        EffectiveToUtc = entity.EffectiveToUtc
    };
}
