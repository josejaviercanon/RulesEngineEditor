using System.Text.Json;
using AutoMapper;
using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
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
    IMapper mapper)
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

        var workflowDto = JsonSerializer.Deserialize<WorkflowDto>(workflowRecord.RuleJson);
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

        workflowDto = new WorkflowDto
        {
            Id = workflowRecord.Id,
            WorkflowName = workflowDto.WorkflowName,
            RuleExpressionType = workflowDto.RuleExpressionType,
            GlobalParams = workflowDto.GlobalParams,
            Rules = activeRules.Select(WorkflowDtoProjection.MapRule).ToArray(),
            WorkflowsToInject = workflowDto.WorkflowsToInject,
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
            var executionResults = await rulesEngineWorkflowService.ExecuteWorkflowAsync(
                workflowRecord.Id,
                workflowDefinition,
                ruleParameters,
                cancellationToken);
            var wasSuccessful = executionResults.All(result => result.IsSuccess);
            var results = mapper.Map<IReadOnlyList<RuleResultDto>>(executionResults);
            var serializedResults = JsonSerializer.Serialize(results);

            if (request.DryRun)
            {
                return new ExecuteWorkflowResultDto
                {
                    IsSuccess = true,
                    DryRun = true,
                    Persisted = false,
                    WasSuccessful = wasSuccessful,
                    Results = results,
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
                Results = results,
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
}
