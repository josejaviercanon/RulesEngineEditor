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
        var records = await dbContext.Workflows
            .AsNoTracking()
            .Select(entity => new WorkflowRecord
            {
                Id = entity.Id,
                Name = entity.Name,
                Expression = entity.Definition.Expression,
                RuleJson = entity.Definition.RuleJson,
                Version = entity.Version,
                IsActive = entity.IsActive,
                EffectiveFromUtc = entity.EffectiveFromUtc,
                EffectiveToUtc = entity.EffectiveToUtc
            })
            .ToListAsync(cancellationToken);

        return records;
    }

    public async Task<WorkflowRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var record = await dbContext.Workflows
            .AsNoTracking()
            .FirstOrDefaultAsync(workflow => workflow.Id == id, cancellationToken);

        return record is null ? null : MapToCore(record);
    }

    public async Task<WorkflowRecord> CreateAsync(WorkflowRecord workflow, CancellationToken cancellationToken)
    {
        var entity = new WorkflowEntity
        {
            Id = workflow.Id == Guid.Empty ? Guid.NewGuid() : workflow.Id,
            Name = workflow.Name,
            Version = workflow.Version,
            IsActive = workflow.IsActive,
            EffectiveFromUtc = workflow.EffectiveFromUtc,
            EffectiveToUtc = workflow.EffectiveToUtc,
            Definition = new WorkflowDefinitionEntity
            {
                Expression = workflow.Expression,
                RuleJson = workflow.RuleJson
            }
        };

        dbContext.Workflows.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToCore(entity);
    }

    public async Task<WorkflowRecord?> UpdateAsync(Guid id, WorkflowRecord workflow, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Workflows
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (entity is null)
        {
            return null;
        }

        entity.Name = workflow.Name;
        entity.Version = workflow.Version;
        entity.IsActive = workflow.IsActive;
        entity.EffectiveFromUtc = workflow.EffectiveFromUtc;
        entity.EffectiveToUtc = workflow.EffectiveToUtc;
        entity.Definition = new WorkflowDefinitionEntity
        {
            Expression = workflow.Expression,
            RuleJson = workflow.RuleJson
        };

        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToCore(entity);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Workflows
            .FirstOrDefaultAsync(workflow => workflow.Id == id, cancellationToken);

        if (entity is null)
        {
            return false;
        }

        dbContext.Workflows.Remove(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return true;
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
