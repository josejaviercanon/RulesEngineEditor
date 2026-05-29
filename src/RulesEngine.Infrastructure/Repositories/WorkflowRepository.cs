using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Infrastructure.Persistence.Entities;

namespace RulesEngine.Infrastructure.Repositories;

public sealed class WorkflowRepository(RulesEngineEditorDbContext dbContext) : IWorkflowRepository
{
    public async Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(
        CancellationToken cancellationToken,
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly)
    {
        var records = await LoadRecordsAsync(cancellationToken);
        return records
            .GroupBy(record => record.Id)
            .Select(group => group
                .OrderByDescending(record => record.IsActive)
                .ThenByDescending(record => record.Version)
                .First())
            .Select(record =>
            {
                record.RuleQueryMode = ruleQueryMode;
                return record;
            })
            .OrderBy(record => record.Name)
            .ThenBy(record => record.Id)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<WorkflowRecord>> ListVersionsAsync(
        Guid id,
        CancellationToken cancellationToken,
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly)
        => (await LoadRecordsAsync(cancellationToken, id))
            .Select(record =>
            {
                record.RuleQueryMode = ruleQueryMode;
                return record;
            })
            .OrderBy(record => record.Version)
            .ToArray();

    public async Task<WorkflowRecord?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken,
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly)
    {
        var records = await LoadRecordsAsync(cancellationToken, id);
        var record = records
            .OrderByDescending(record => record.IsActive)
            .ThenByDescending(record => record.Version)
            .FirstOrDefault();

        if (record is not null)
        {
            record.RuleQueryMode = ruleQueryMode;
        }

        return record;
    }

    public async Task<WorkflowRecord?> GetVersionAsync(
        Guid id,
        int version,
        CancellationToken cancellationToken,
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly)
    {
        var record = (await LoadRecordsAsync(cancellationToken, id))
            .FirstOrDefault(workflow => workflow.Version == version);

        if (record is not null)
        {
            record.RuleQueryMode = ruleQueryMode;
        }

        return record;
    }

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
        await UpsertWorkflowRulesAsync(workflowId, nextVersion, workflow.RuleJson, cancellationToken);
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
        await UpsertWorkflowRulesAsync(id, nextVersion, workflow.RuleJson, cancellationToken);

        await SaveChangesAsync(cancellationToken);

        return MapToCore(entityToAdd);
    }

    public async Task<IReadOnlyCollection<RuleVersionRecord>> ListRuleVersionsAsync(
        Guid workflowId,
        Guid ruleGuidId,
        CancellationToken cancellationToken)
    {
        var isOwnedByWorkflow = await dbContext.WorkflowRules
            .AnyAsync(rule => rule.WorkflowId == workflowId && rule.RuleGuidId == ruleGuidId, cancellationToken);

        if (!isOwnedByWorkflow)
        {
            return [];
        }

        var versions = await dbContext.Rules
            .AsNoTracking()
            .Where(rule => rule.RuleGuidId == ruleGuidId)
            .OrderBy(rule => rule.Version)
            .ToListAsync(cancellationToken);

        return versions.Select(MapRuleToCore).ToArray();
    }

    public async Task<RuleVersionRecord?> ActivateRuleVersionAsync(
        Guid workflowId,
        Guid ruleGuidId,
        int version,
        CancellationToken cancellationToken)
    {
        var isOwnedByWorkflow = await dbContext.WorkflowRules
            .AnyAsync(rule => rule.WorkflowId == workflowId && rule.RuleGuidId == ruleGuidId, cancellationToken);

        if (!isOwnedByWorkflow)
        {
            return null;
        }

        var entities = await dbContext.Rules
            .Where(rule => rule.RuleGuidId == ruleGuidId)
            .ToListAsync(cancellationToken);

        if (entities.Count == 0)
        {
            return null;
        }

        var target = entities.FirstOrDefault(rule => rule.Version == version);
        if (target is null)
        {
            return null;
        }

        foreach (var entity in entities)
        {
            entity.IsActive = entity.Version == version;
        }

        await SaveChangesAsync(cancellationToken);
        return MapRuleToCore(target);
    }

    public async Task<IReadOnlyCollection<RuleVersionRecord>> ListWorkflowRulesAsync(
        Guid workflowId,
        int workflowVersion,
        WorkflowRuleQueryMode mode,
        CancellationToken cancellationToken)
    {
        var ruleGuids = await dbContext.WorkflowRules
            .AsNoTracking()
            .Where(rule => rule.WorkflowId == workflowId && rule.WorkflowVersion == workflowVersion)
            .Select(rule => rule.RuleGuidId)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (ruleGuids.Count == 0)
        {
            var workflow = await dbContext.Workflows
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.Id == workflowId && item.Version == workflowVersion, cancellationToken);

            if (workflow is null)
            {
                return [];
            }

            return ParseTopLevelRules(workflow.Definition.RuleJson)
                .Select(item => new RuleVersionRecord
                {
                    Id = Guid.Empty,
                    RuleGuidId = item.RuleGuidId == Guid.Empty
                        ? CreateDeterministicGuid(workflowId, item.RuleName, item.Expression)
                        : item.RuleGuidId,
                    Name = item.RuleName,
                    Expression = item.Expression,
                    RuleJson = item.RawRuleJson,
                    Version = 1,
                    IsActive = true
                })
                .ToArray();
        }

        var query = dbContext.Rules
            .AsNoTracking()
            .Where(rule => ruleGuids.Contains(rule.RuleGuidId));

        List<RuleRecord> entities;
        switch (mode)
        {
            case WorkflowRuleQueryMode.ActiveOnly:
                entities = await query
                    .Where(rule => rule.IsActive)
                    .OrderBy(rule => rule.Name)
                    .ThenBy(rule => rule.RuleGuidId)
                    .ToListAsync(cancellationToken);
                break;
            case WorkflowRuleQueryMode.LatestPerRule:
                entities = await query
                    .GroupBy(rule => rule.RuleGuidId)
                    .Select(group => group
                        .OrderByDescending(item => item.Version)
                        .First())
                    .OrderBy(rule => rule.Name)
                    .ThenBy(rule => rule.RuleGuidId)
                    .ToListAsync(cancellationToken);
                break;
            default:
                entities = await query
                    .OrderBy(rule => rule.Name)
                    .ThenBy(rule => rule.RuleGuidId)
                    .ThenBy(rule => rule.Version)
                    .ToListAsync(cancellationToken);
                break;
        }

        return entities.Select(MapRuleToCore).ToArray();
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
        await CleanupOrphanedRulesAsync(cancellationToken);
        await SaveChangesAsync(cancellationToken);

        return true;
    }

    private async Task CleanupOrphanedRulesAsync(CancellationToken cancellationToken)
    {
        var referencedRuleGuids = await dbContext.WorkflowRules
            .AsNoTracking()
            .Select(rule => rule.RuleGuidId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var orphaned = await dbContext.Rules
            .Where(rule => !referencedRuleGuids.Contains(rule.RuleGuidId))
            .ToListAsync(cancellationToken);

        if (orphaned.Count > 0)
        {
            dbContext.Rules.RemoveRange(orphaned);
        }
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

    private async Task<int> GetNextRuleVersionAsync(Guid ruleGuidId, CancellationToken cancellationToken)
    {
        var latestVersion = await dbContext.Rules
            .Where(rule => rule.RuleGuidId == ruleGuidId)
            .Select(rule => (int?)rule.Version)
            .MaxAsync(cancellationToken);

        return (latestVersion ?? 0) + 1;
    }

    private async Task<Dictionary<string, Guid>> LoadActiveRuleGuidsByNameAsync(Guid workflowId, CancellationToken cancellationToken)
    {
        return await dbContext.WorkflowRules
            .AsNoTracking()
            .Where(link => link.WorkflowId == workflowId)
            .Join(
                dbContext.Rules.AsNoTracking().Where(rule => rule.IsActive),
                link => link.RuleGuidId,
                rule => rule.RuleGuidId,
                (_, rule) => new { rule.Name, rule.RuleGuidId })
            .Distinct()
            .ToDictionaryAsync(item => item.Name, item => item.RuleGuidId, StringComparer.Ordinal, cancellationToken);
    }

    private async Task UpsertWorkflowRulesAsync(
        Guid workflowId,
        int workflowVersion,
        string workflowJson,
        CancellationToken cancellationToken)
    {
        var rules = ParseTopLevelRules(workflowJson);
        if (rules.Count == 0)
        {
            return;
        }

        var activeByName = await LoadActiveRuleGuidsByNameAsync(workflowId, cancellationToken);
        var resolved = new List<(ParsedRulePayload Rule, Guid RuleGuidId)>();
        foreach (var rule in rules)
        {
            var ruleGuidId = rule.RuleGuidId;
            if (ruleGuidId == Guid.Empty)
            {
                if (!activeByName.TryGetValue(rule.RuleName, out ruleGuidId))
                {
                    ruleGuidId = CreateDeterministicGuid(workflowId, rule.RuleName, rule.Expression);
                }
            }

            resolved.Add((rule, ruleGuidId));
        }

        foreach (var item in resolved.GroupBy(x => x.RuleGuidId).Select(group => group.Last()))
        {
            var nextVersion = await GetNextRuleVersionAsync(item.RuleGuidId, cancellationToken);

            var currentlyActive = await dbContext.Rules
                .Where(rule => rule.RuleGuidId == item.RuleGuidId && rule.IsActive)
                .ToListAsync(cancellationToken);

            foreach (var active in currentlyActive)
            {
                active.IsActive = false;
            }

            var record = new RuleRecord
            {
                Id = Guid.NewGuid(),
                RuleGuidId = item.RuleGuidId,
                Name = item.Rule.RuleName,
                Expression = item.Rule.Expression,
                RuleJson = item.Rule.RawRuleJson,
                Version = nextVersion,
                IsActive = true,
                EffectiveFromUtc = null,
                EffectiveToUtc = null
            };

            dbContext.Rules.Add(record);
            dbContext.WorkflowRules.Add(new WorkflowRuleCollectionEntity
            {
                WorkflowId = workflowId,
                WorkflowVersion = workflowVersion,
                RuleGuidId = item.RuleGuidId,
                RuleVersion = nextVersion
            });
        }
    }

    private static List<ParsedRulePayload> ParseTopLevelRules(string workflowJson)
    {
        try
        {
            using var document = JsonDocument.Parse(workflowJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return [];
            }

            if (!document.RootElement.TryGetProperty("Rules", out var rulesElement) || rulesElement.ValueKind != JsonValueKind.Array)
            {
                return [];
            }

            var rules = new List<ParsedRulePayload>();
            foreach (var item in rulesElement.EnumerateArray())
            {
                if (item.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                var ruleName = item.TryGetProperty("RuleName", out var ruleNameElement)
                    ? (ruleNameElement.GetString() ?? string.Empty)
                    : string.Empty;
                var expression = item.TryGetProperty("Expression", out var expressionElement)
                    ? (expressionElement.GetString() ?? string.Empty)
                    : string.Empty;
                var ruleGuidId = Guid.Empty;
                if (item.TryGetProperty("RuleGuidId", out var ruleGuidElement) &&
                    ruleGuidElement.ValueKind == JsonValueKind.String &&
                    Guid.TryParse(ruleGuidElement.GetString(), out var parsed))
                {
                    ruleGuidId = parsed;
                }

                rules.Add(new ParsedRulePayload(ruleName, expression, item.GetRawText(), ruleGuidId));
            }

            return rules;
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static Guid CreateDeterministicGuid(Guid workflowId, string ruleName, string expression)
    {
        var bytes = System.Security.Cryptography.MD5.HashData(
            System.Text.Encoding.UTF8.GetBytes($"{workflowId}:{ruleName}:{expression}"));
        return new Guid(bytes);
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

    private static RuleVersionRecord MapRuleToCore(RuleRecord record) => new()
    {
        Id = record.Id,
        RuleGuidId = record.RuleGuidId,
        Name = record.Name,
        Expression = record.Expression,
        RuleJson = record.RuleJson,
        Version = record.Version,
        IsActive = record.IsActive,
        EffectiveFromUtc = record.EffectiveFromUtc,
        EffectiveToUtc = record.EffectiveToUtc
    };

    private sealed record ParsedRulePayload(string RuleName, string Expression, string RawRuleJson, Guid RuleGuidId);
}
