using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Nodes;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Infrastructure.Persistence.Entities;

namespace RulesEngine.Infrastructure.Repositories;

public sealed class WorkflowRepository(RulesEngineEditorDbContext dbContext) : IWorkflowRepository
{
    public async Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(
        CancellationToken cancellationToken,
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly,
        bool? isEnabled = null)
    {
        var records = await LoadRecordsAsync(cancellationToken);
        return records
            .GroupBy(record => record.Id)
            .Select(group =>
            {
                var representative = group
                    .OrderByDescending(record => record.IsActive)
                    .ThenByDescending(record => record.Version)
                    .First();

                representative.ActiveVersion = group.FirstOrDefault(item => item.IsActive)?.Version ?? representative.Version;
                representative.LastVersion = group.Max(item => item.Version);
                return representative;
            })
            .Where(record => !isEnabled.HasValue || record.IsEnabled == isEnabled.Value)
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
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly,
        bool? isEnabled = null)
        => (await LoadRecordsAsync(cancellationToken, id, isEnabled))
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
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly,
        bool? isEnabled = null)
    {
        var records = await LoadRecordsAsync(cancellationToken, id);
        var record = records
            .OrderByDescending(record => record.IsActive)
            .ThenByDescending(record => record.Version)
            .FirstOrDefault();

        if (record is not null && isEnabled.HasValue && record.IsEnabled != isEnabled.Value)
        {
            return null;
        }

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
        WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly,
        bool? isEnabled = null)
    {
        var record = (await LoadRecordsAsync(cancellationToken, id))
            .FirstOrDefault(workflow => workflow.Version == version);

        if (record is not null && isEnabled.HasValue && record.IsEnabled != isEnabled.Value)
        {
            return null;
        }

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

        if (target.IsActive)
        {
            return MapToCore(target);
        }

        if (dbContext.Database.IsRelational())
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            foreach (var entity in entities.Where(entity => entity.IsActive && entity.Version != version))
            {
                entity.IsActive = false;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            target.IsActive = true;
            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            return MapToCore(target);
        }

        foreach (var entity in entities.Where(entity => entity.IsActive && entity.Version != version))
        {
            entity.IsActive = false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        target.IsActive = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return MapToCore(target);
    }

    public async Task<WorkflowRecord?> SetVersionEnabledAsync(Guid id, int version, bool isEnabled, CancellationToken cancellationToken)
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

        target.IsEnabled = isEnabled;

        if (isEnabled)
        {
            foreach (var entity in entities.Where(entity => entity.Version != version && entity.IsActive))
            {
                entity.IsEnabled = false;
            }
        }

        await SaveChangesAsync(cancellationToken);
        return MapToCore(target);
    }

    public async Task<WorkflowRecord> CreateAsync(WorkflowRecord workflow, CancellationToken cancellationToken)
    {
        var workflowId = workflow.Id == Guid.Empty ? Guid.NewGuid() : workflow.Id;
        var nextVersion = await GetNextVersionAsync(workflowId, cancellationToken);
        var workflowJson = JsonPayloadUtilities.ResolveWorkflowJson(workflow.WorkflowJson, workflow.RuleJson);

        await DeactivateCurrentActiveAsync(workflowId, cancellationToken);

        var entity = new WorkflowEntity
        {
            Id = workflowId,
            Name = workflow.Name,
            Version = nextVersion,
            IsActive = true,
            WorkflowJson = workflowJson,
            IsEnabled = workflow.IsEnabled,
            Comments = workflow.Comments,
            EffectiveFromUtc = workflow.EffectiveFromUtc,
            EffectiveToUtc = workflow.EffectiveToUtc,
            Definition = new WorkflowDefinitionEntity
            {
                Expression = workflow.Expression,
                RuleJson = workflowJson
            }
        };

        dbContext.Workflows.Add(entity);
        await UpsertWorkflowRulesAsync(workflowId, nextVersion, workflowJson, cancellationToken);
        await SaveChangesAsync(cancellationToken);

        var created = MapToCore(entity);
        created.ActiveVersion = created.Version;
        created.LastVersion = created.Version;
        return created;
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

        var target = workflow.Version > 0
            ? existing.FirstOrDefault(entity => entity.Version == workflow.Version)
            : existing.FirstOrDefault(entity => entity.IsActive);

        if (target is null)
        {
            return null;
        }

        var workflowJson = JsonPayloadUtilities.ResolveWorkflowJson(workflow.WorkflowJson, workflow.RuleJson);

        target.Name = workflow.Name;
        target.WorkflowJson = workflowJson;
        target.IsEnabled = workflow.IsEnabled;
        target.Comments = workflow.Comments;
        target.EffectiveFromUtc = workflow.EffectiveFromUtc;
        target.EffectiveToUtc = workflow.EffectiveToUtc;
        target.Definition ??= new WorkflowDefinitionEntity();
        target.Definition.Expression = workflow.Expression;
        target.Definition.RuleJson = workflowJson;

        await SyncWorkflowRulesAsync(id, target.Version, workflowJson, cancellationToken);

        await SaveChangesAsync(cancellationToken);

        var updated = MapToCore(target);
        updated.ActiveVersion = existing.FirstOrDefault(entity => entity.IsActive)?.Version ?? target.Version;
        updated.LastVersion = existing.Max(entity => entity.Version);
        return updated;
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

        var activeVersion = versions.FirstOrDefault(rule => rule.IsActive)?.Version ?? 0;
        var lastVersion = versions.Count == 0 ? 0 : versions.Max(rule => rule.Version);

        return versions.Select(record =>
        {
            var mapped = MapRuleToCore(record);
            mapped.ActiveVersion = activeVersion;
            mapped.LastVersion = lastVersion;
            return mapped;
        }).ToArray();
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

        if (target.IsActive)
        {
            var mappedTarget = MapRuleToCore(target);
            mappedTarget.ActiveVersion = target.Version;
            mappedTarget.LastVersion = entities.Max(item => item.Version);
            return mappedTarget;
        }

        if (dbContext.Database.IsRelational())
        {
            await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            foreach (var entity in entities.Where(rule => rule.IsActive && rule.Version != version))
            {
                entity.IsActive = false;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            target.IsActive = true;
            await dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
            var mappedTarget = MapRuleToCore(target);
            mappedTarget.ActiveVersion = target.Version;
            mappedTarget.LastVersion = entities.Max(item => item.Version);
            return mappedTarget;
        }

        foreach (var entity in entities.Where(rule => rule.IsActive && rule.Version != version))
        {
            entity.IsActive = false;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        target.IsActive = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        var mapped = MapRuleToCore(target);
        mapped.ActiveVersion = target.Version;
        mapped.LastVersion = entities.Max(item => item.Version);
        return mapped;
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

            return ParseTopLevelRules(JsonPayloadUtilities.ResolveWorkflowJson(workflow.WorkflowJson, workflow.Definition.RuleJson))
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
                    ActiveVersion = 1,
                    LastVersion = 1,
                    IsActive = true,
                    Status = item.Status
                })
                .ToArray();
        }

        var query = dbContext.Rules
            .AsNoTracking()
            .Where(rule => ruleGuids.Contains(rule.RuleGuidId));

        var ruleVersionSummary = await query
            .GroupBy(rule => rule.RuleGuidId)
            .Select(group => new
            {
                RuleGuidId = group.Key,
                ActiveVersion = group.Where(item => item.IsActive).Select(item => item.Version).FirstOrDefault(),
                LastVersion = group.Max(item => item.Version)
            })
            .ToDictionaryAsync(item => item.RuleGuidId, cancellationToken);

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

        return entities.Select(record =>
        {
            var mapped = MapRuleToCore(record);
            if (ruleVersionSummary.TryGetValue(record.RuleGuidId, out var summary))
            {
                mapped.ActiveVersion = summary.ActiveVersion;
                mapped.LastVersion = summary.LastVersion;
            }

            return mapped;
        }).ToArray();
    }

    public async Task ApplyRuleStatusUpdatesAsync(
        IReadOnlyCollection<RuleStatusUpdateRecord> updates,
        CancellationToken cancellationToken)
    {
        if (updates.Count == 0)
        {
            return;
        }

        var byGuid = updates
            .GroupBy(update => update.RuleGuidId)
            .ToDictionary(group => group.Key, group => group.Last().Status);

        var ruleGuids = byGuid.Keys.ToArray();
        var activeRules = await dbContext.Rules
            .Where(rule => ruleGuids.Contains(rule.RuleGuidId) && rule.IsActive)
            .ToListAsync(cancellationToken);

        foreach (var active in activeRules)
        {
            if (!byGuid.TryGetValue(active.RuleGuidId, out var nextStatus))
            {
                continue;
            }

            active.Status = nextStatus;
            active.RuleJson = TryWriteRuleStatus(active.RuleJson, nextStatus);
        }

        await SaveChangesAsync(cancellationToken);
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
                RuleJson = JsonPayloadUtilities.EnsureRuleJsonContainsExpression(item.Rule.RawRuleJson, item.Rule.Expression),
                Version = nextVersion,
                IsActive = true,
                Status = item.Rule.Status,
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

    private async Task SyncWorkflowRulesAsync(
        Guid workflowId,
        int workflowVersion,
        string workflowJson,
        CancellationToken cancellationToken)
    {
        var rules = ParseTopLevelRules(workflowJson);

        var existingLinks = await dbContext.WorkflowRules
            .Where(link => link.WorkflowId == workflowId && link.WorkflowVersion == workflowVersion)
            .ToListAsync(cancellationToken);

        dbContext.WorkflowRules.RemoveRange(existingLinks);

        if (rules.Count == 0)
        {
            return;
        }

        var activeByName = await LoadActiveRuleGuidsByNameAsync(workflowId, cancellationToken);

        var desiredLinks = new List<(Guid RuleGuidId, int RuleVersion)>();

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

            var resolvedVersion = rule.Version > 0
                ? rule.Version
                : await GetNextRuleVersionAsync(ruleGuidId, cancellationToken);

            var ruleRecord = await dbContext.Rules
                .FirstOrDefaultAsync(item => item.RuleGuidId == ruleGuidId && item.Version == resolvedVersion, cancellationToken);

            if (ruleRecord is null)
            {
                var currentlyActive = await dbContext.Rules
                    .Where(item => item.RuleGuidId == ruleGuidId && item.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var active in currentlyActive)
                {
                    active.IsActive = false;
                }

                ruleRecord = new RuleRecord
                {
                    Id = Guid.NewGuid(),
                    RuleGuidId = ruleGuidId,
                    Name = rule.RuleName,
                    Expression = rule.Expression,
                    RuleJson = JsonPayloadUtilities.EnsureRuleJsonContainsExpression(rule.RawRuleJson, rule.Expression),
                    Version = resolvedVersion,
                    IsActive = true,
                    Status = rule.Status,
                    EffectiveFromUtc = null,
                    EffectiveToUtc = null
                };

                dbContext.Rules.Add(ruleRecord);
            }
            else
            {
                ruleRecord.Name = rule.RuleName;
                ruleRecord.Expression = rule.Expression;
                ruleRecord.RuleJson = JsonPayloadUtilities.EnsureRuleJsonContainsExpression(rule.RawRuleJson, rule.Expression);
                ruleRecord.Status = rule.Status;

                if (rule.IsActive)
                {
                    var currentlyActive = await dbContext.Rules
                        .Where(item => item.RuleGuidId == ruleGuidId && item.IsActive && item.Version != resolvedVersion)
                        .ToListAsync(cancellationToken);

                    foreach (var active in currentlyActive)
                    {
                        active.IsActive = false;
                    }

                    ruleRecord.IsActive = true;
                }
            }

            desiredLinks.Add((ruleGuidId, resolvedVersion));
        }

        var desiredByRuleGuid = desiredLinks
            .GroupBy(item => item.RuleGuidId)
            .Select(group => group.Last())
            .ToDictionary(item => item.RuleGuidId, item => item.RuleVersion);

        foreach (var link in existingLinks)
        {
            if (!desiredByRuleGuid.ContainsKey(link.RuleGuidId))
            {
                dbContext.WorkflowRules.Remove(link);
            }
        }

        foreach (var item in desiredByRuleGuid)
        {
            var existingLink = existingLinks.FirstOrDefault(link => link.RuleGuidId == item.Key);
            if (existingLink is null)
            {
                dbContext.WorkflowRules.Add(new WorkflowRuleCollectionEntity
                {
                    WorkflowId = workflowId,
                    WorkflowVersion = workflowVersion,
                    RuleGuidId = item.Key,
                    RuleVersion = item.Value
                });
                continue;
            }

            existingLink.RuleVersion = item.Value;
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
                var status = RuleStatus.Draft;
                var version = 1;
                var isActive = true;
                if (item.TryGetProperty("RuleGuidId", out var ruleGuidElement) &&
                    ruleGuidElement.ValueKind == JsonValueKind.String &&
                    Guid.TryParse(ruleGuidElement.GetString(), out var parsed))
                {
                    ruleGuidId = parsed;
                }

                if (item.TryGetProperty("Version", out var versionElement) &&
                    versionElement.ValueKind == JsonValueKind.Number &&
                    versionElement.TryGetInt32(out var parsedVersion))
                {
                    version = parsedVersion;
                }

                if (item.TryGetProperty("IsActive", out var isActiveElement) &&
                    (isActiveElement.ValueKind == JsonValueKind.True || isActiveElement.ValueKind == JsonValueKind.False))
                {
                    isActive = isActiveElement.GetBoolean();
                }

                if (item.TryGetProperty("Status", out var statusElement) &&
                    statusElement.ValueKind == JsonValueKind.String)
                {
                    status = RuleStatusParser.ParseOrDefault(statusElement.GetString(), RuleStatus.Draft);
                }

                rules.Add(new ParsedRulePayload(ruleName, expression, item.GetRawText(), ruleGuidId, status, version, isActive));
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

    private async Task<List<WorkflowRecord>> LoadRecordsAsync(CancellationToken cancellationToken, Guid? workflowId = null, bool? isEnabled = null)
    {
        var query = dbContext.Workflows.AsNoTracking().AsQueryable();
        if (workflowId.HasValue)
        {
            query = query.Where(workflow => workflow.Id == workflowId.Value);
        }

        if (isEnabled.HasValue)
        {
            query = query.Where(workflow => workflow.IsEnabled == isEnabled.Value);
        }

        var entities = await query.ToListAsync(cancellationToken);
        return entities.Select(MapToCore).ToList();
    }

    private static WorkflowRecord MapToCore(WorkflowEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Expression = entity.Definition.Expression,
        WorkflowJson = JsonPayloadUtilities.ResolveWorkflowJson(entity.WorkflowJson, entity.Definition.RuleJson),
        RuleJson = JsonPayloadUtilities.ResolveWorkflowJson(entity.WorkflowJson, entity.Definition.RuleJson),
        Version = entity.Version,
        ActiveVersion = entity.IsActive ? entity.Version : 0,
        LastVersion = entity.Version,
        IsActive = entity.IsActive,
        IsEnabled = entity.IsEnabled,
        Comments = entity.Comments,
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
        ActiveVersion = record.IsActive ? record.Version : 0,
        LastVersion = record.Version,
        IsActive = record.IsActive,
        Status = record.Status,
        EffectiveFromUtc = record.EffectiveFromUtc,
        EffectiveToUtc = record.EffectiveToUtc
    };

    private static string TryWriteRuleStatus(string ruleJson, RuleStatus status)
    {
        try
        {
            var node = JsonNode.Parse(ruleJson);
            if (node is not JsonObject obj)
            {
                return ruleJson;
            }

            obj["Status"] = RuleStatusParser.ToValue(status);
            return obj.ToJsonString();
        }
        catch (JsonException)
        {
            return ruleJson;
        }
    }

    private sealed record ParsedRulePayload(
        string RuleName,
        string Expression,
        string RawRuleJson,
        Guid RuleGuidId,
        RuleStatus Status,
        int Version,
        bool IsActive);
}
