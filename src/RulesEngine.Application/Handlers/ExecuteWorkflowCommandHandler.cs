using System.Text.Json;
using AutoMapper;
using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Application.Policies;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;
using RulesEngine.Exceptions;
using RulesEngine.Models;

namespace RulesEngine.Application.Handlers;

public sealed class ExecuteWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IRulesEngineWorkflowService rulesEngineWorkflowService,
    IExecutionStateRepository executionStateRepository,
    IMapper mapper,
    IRuleStatusPolicy ruleStatusPolicy)
    : IRequestHandler<ExecuteWorkflowCommand, ExecuteWorkflowResultDto>
{
    public async Task<ExecuteWorkflowResultDto> Handle(ExecuteWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workflowRecord = await workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
        if (workflowRecord is null)
        {
            return new ExecuteWorkflowResultDto
            {
                Found = false,
                IsSuccess = false
            };
        }

        var workflowJson = !string.IsNullOrWhiteSpace(workflowRecord.WorkflowJson)
            ? workflowRecord.WorkflowJson
            : workflowRecord.RuleJson;
        var workflowDto = JsonSerializer.Deserialize<WorkflowDto>(workflowJson);
        if (workflowDto is null)
        {
            return new ExecuteWorkflowResultDto
            {
                IsSuccess = false,
                SchemaVersion = request.SchemaVersion,
                ErrorCode = "deserialization_failed",
                ErrorMessage = "Stored workflow payload could not be deserialized as WorkflowDto."
            };
        }

        var activeRules = await workflowRepository.ListWorkflowRulesAsync(
            workflowRecord.Id,
            workflowRecord.Version,
            WorkflowRuleQueryMode.ActiveOnly,
            cancellationToken);

        var includeStatuses = ResolveIncludedStatuses(request.IncludeStatuses, out var statusFilterError);
        if (statusFilterError is not null)
        {
            return new ExecuteWorkflowResultDto
            {
                IsSuccess = false,
                SchemaVersion = request.SchemaVersion,
                ErrorCode = "invalid_status_filter",
                ErrorMessage = statusFilterError
            };
        }

        var selectedRules = activeRules
            .Where(rule => includeStatuses.Contains(rule.Status) && rule.Status != RuleStatus.Disabled)
            .ToArray();

        workflowDto = new WorkflowDto
        {
            Id = workflowRecord.Id,
            WorkflowName = workflowDto.WorkflowName,
            RuleExpressionType = workflowDto.RuleExpressionType,
            GlobalParams = workflowDto.GlobalParams,
            Rules = selectedRules.Select(WorkflowDtoProjection.MapRule).ToArray(),
            WorkflowsToInject = workflowDto.WorkflowsToInject,
            WorkflowJson = workflowJson,
            Version = workflowRecord.Version,
            IsActive = workflowRecord.IsActive,
            EffectiveFromUtc = workflowRecord.EffectiveFromUtc,
            EffectiveToUtc = workflowRecord.EffectiveToUtc
        };

        RuleParameter[] ruleParameters;
        try
        {
            ruleParameters = request.Inputs.Select(input =>
            {
                var json = JsonSerializer.Deserialize<JsonElement>(input.ValueJson);
                return new RuleParameter(input.Name, json);
            }).ToArray();
        }
        catch (JsonException exception)
        {
            return new ExecuteWorkflowResultDto
            {
                IsSuccess = false,
                SchemaVersion = request.SchemaVersion,
                ErrorCode = "invalid_input_json",
                ErrorMessage = "One or more input parameters contain invalid JSON.",
                Errors = [exception.Message]
            };
        }

        var workflowDefinition = mapper.Map<Workflow>(workflowDto);

        try
        {
            var executionResults = selectedRules.Length == activeRules.Count
                ? await rulesEngineWorkflowService.ExecuteWorkflowAsync(
                    workflowRecord.Id,
                    workflowDefinition,
                    ruleParameters,
                    cancellationToken)
                : await ExecuteTransientAsync(workflowDefinition, ruleParameters);
            var wasSuccessful = executionResults.All(result => result.IsSuccess);
            var mappedResults = mapper.Map<IReadOnlyList<RuleResultDto>>(executionResults);

            var byRuleName = selectedRules
                .GroupBy(rule => rule.Name, StringComparer.Ordinal)
                .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);

            var statusUpdates = new Dictionary<Guid, RuleStatusUpdateRecord>();
            var transitions = new List<RuleStatusTransitionDto>();

            var enrichedResults = mappedResults
                .Select(result => EnrichResult(result, byRuleName, statusUpdates, transitions))
                .ToArray();

            if (!request.DryRun && statusUpdates.Count > 0)
            {
                await workflowRepository.ApplyRuleStatusUpdatesAsync(statusUpdates.Values.ToArray(), cancellationToken);
            }

            var serializedResults = JsonSerializer.Serialize(enrichedResults);

            if (request.DryRun)
            {
                return new ExecuteWorkflowResultDto
                {
                    IsSuccess = true,
                    DryRun = true,
                    Persisted = false,
                    WasSuccessful = wasSuccessful,
                    Results = enrichedResults,
                    RuleStatusTransitions = transitions,
                    SchemaVersion = request.SchemaVersion,
                    ExecutionId = null
                };
            }

            var executionId = await executionStateRepository.CreateAsync(new ExecutionStateRecord
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowRecord.Id,
                IsDryRun = false,
                WasSuccessful = wasSuccessful,
                ExecutedAtUtc = DateTimeOffset.UtcNow,
                ResultJson = serializedResults,
                ErrorJson = null
            }, cancellationToken);

            return new ExecuteWorkflowResultDto
            {
                IsSuccess = true,
                DryRun = false,
                Persisted = true,
                WasSuccessful = wasSuccessful,
                Results = enrichedResults,
                RuleStatusTransitions = transitions,
                SchemaVersion = request.SchemaVersion,
                ExecutionId = executionId
            };
        }
        catch (RuleValidationException exception)
        {
            return new ExecuteWorkflowResultDto
            {
                IsSuccess = false,
                SchemaVersion = request.SchemaVersion,
                ErrorCode = "validation_failed",
                ErrorMessage = exception.Message,
                Errors = exception.Errors.Select(error => error.ErrorMessage).ToArray()
            };
        }
        catch (Exception exception)
        {
            return new ExecuteWorkflowResultDto
            {
                IsSuccess = false,
                SchemaVersion = request.SchemaVersion,
                ErrorCode = "execution_failed",
                ErrorMessage = exception.Message,
                Errors = null
            };
        }
    }

    private RuleResultDto EnrichResult(
        RuleResultDto result,
        IReadOnlyDictionary<string, RuleVersionRecord> byRuleName,
        IDictionary<Guid, RuleStatusUpdateRecord> statusUpdates,
        ICollection<RuleStatusTransitionDto> transitions)
    {
        var enrichedChildren = result.ChildResults
            .Select(child => EnrichResult(child, byRuleName, statusUpdates, transitions))
            .ToArray();

        if (!byRuleName.TryGetValue(result.RuleName, out var ruleRecord))
        {
            return new RuleResultDto
            {
                RuleGuidId = result.RuleGuidId,
                RuleName = result.RuleName,
                IsSuccess = result.IsSuccess,
                ExceptionMessage = result.ExceptionMessage,
                SuccessEvent = result.SuccessEvent,
                ActionOutput = result.ActionOutput,
                StatusBefore = result.StatusBefore,
                StatusAfter = result.StatusAfter,
                TransitionReason = result.TransitionReason,
                ChildResults = enrichedChildren
            };
        }

        var before = ruleRecord.Status;
        var after = result.IsSuccess ? before : ruleStatusPolicy.ResolveExecutionFailureStatus(before);
        var reason = !result.IsSuccess && before != after
            ? "execution_failed_auto_transition"
            : null;

        if (before != after)
        {
            statusUpdates[ruleRecord.RuleGuidId] = new RuleStatusUpdateRecord
            {
                RuleGuidId = ruleRecord.RuleGuidId,
                Status = after
            };
        }

        transitions.Add(new RuleStatusTransitionDto
        {
            RuleGuidId = ruleRecord.RuleGuidId,
            RuleName = ruleRecord.Name,
            StatusBefore = RuleStatusParser.ToValue(before),
            StatusAfter = RuleStatusParser.ToValue(after),
            TransitionReason = reason
        });

        return new RuleResultDto
        {
            RuleGuidId = ruleRecord.RuleGuidId,
            RuleName = result.RuleName,
            IsSuccess = result.IsSuccess,
            ExceptionMessage = result.ExceptionMessage,
            SuccessEvent = result.SuccessEvent,
            ActionOutput = result.ActionOutput,
            StatusBefore = RuleStatusParser.ToValue(before),
            StatusAfter = RuleStatusParser.ToValue(after),
            TransitionReason = reason,
            ChildResults = enrichedChildren
        };
    }

    private HashSet<RuleStatus> ResolveIncludedStatuses(IReadOnlyList<string>? requestedStatuses, out string? error)
    {
        error = null;

        if (requestedStatuses is null || requestedStatuses.Count == 0)
        {
            return [RuleStatus.Draft, RuleStatus.Failed, RuleStatus.Production];
        }

        var included = new HashSet<RuleStatus>();
        foreach (var statusValue in requestedStatuses)
        {
            if (!RuleStatusParser.TryParse(statusValue, out var parsed))
            {
                error = $"Unknown rule status filter value '{statusValue}'. Allowed values: draft, failed, production.";
                return [];
            }

            if (parsed == RuleStatus.Disabled)
            {
                continue;
            }

            included.Add(parsed);
        }

        if (included.Count == 0)
        {
            included.Add(RuleStatus.Draft);
            included.Add(RuleStatus.Failed);
            included.Add(RuleStatus.Production);
        }

        return included;
    }

    private static async Task<IReadOnlyList<RuleResultTree>> ExecuteTransientAsync(Workflow workflow, RuleParameter[] ruleParameters)
    {
        var engine = new global::RulesEngine.RulesEngine(new ReSettings
        {
            EnableExceptionAsErrorMessage = true,
            EnableExceptionAsErrorMessageForRuleExpressionParsing = true
        });

        engine.AddOrUpdateWorkflow(workflow);
        var results = await engine.ExecuteAllRulesAsync(workflow.WorkflowName, ruleParameters);
        return results.AsReadOnly();
    }
}
