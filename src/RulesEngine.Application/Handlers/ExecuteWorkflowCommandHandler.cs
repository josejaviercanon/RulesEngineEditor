using System.Text.Json;
using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;
using RulesEngine.Core.Validation;

namespace RulesEngine.Application.Handlers;

public sealed class ExecuteWorkflowCommandHandler(
    IWorkflowRepository workflowRepository,
    IWorkflowSchemaValidator schemaValidator,
    IRulesEngineWorkflowService rulesEngineWorkflowService,
    IExecutionStateRepository executionStateRepository)
    : IRequestHandler<ExecuteWorkflowCommand, ExecuteWorkflowResultDto>
{
    public async Task<ExecuteWorkflowResultDto> Handle(ExecuteWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workflow = await workflowRepository.GetByIdAsync(request.WorkflowId, cancellationToken);
        if (workflow is null)
        {
            return new ExecuteWorkflowResultDto
            {
                Found = false,
                IsSuccess = false
            };
        }

        var validationResult = schemaValidator.Validate(workflow.RuleJson, request.SchemaVersion);
        if (!validationResult.IsValid)
        {
            return new ExecuteWorkflowResultDto
            {
                IsSuccess = false,
                SchemaVersion = validationResult.ResolvedVersion,
                ErrorCode = "validation_failed",
                ErrorMessage = "Workflow schema validation failed.",
                Errors = validationResult.Errors
            };
        }

        try
        {
            rulesEngineWorkflowService.AddOrUpdateWorkflow(workflow.RuleJson);
            var executionResults = await rulesEngineWorkflowService.ExecuteAllRulesAsync(workflow.Name);

            var resultPayload = executionResults
                .Select(result => new
                {
                    RuleName = result.Rule?.RuleName,
                    result.IsSuccess,
                    result.ExceptionMessage
                })
                .OrderBy(result => result.RuleName, StringComparer.Ordinal)
                .ToArray();

            var resultJson = JsonSerializer.Serialize(resultPayload);
            var wasSuccessful = executionResults.All(result => result.IsSuccess);

            if (request.DryRun)
            {
                return new ExecuteWorkflowResultDto
                {
                    IsSuccess = true,
                    DryRun = true,
                    Persisted = false,
                    WasSuccessful = wasSuccessful,
                    ResultJson = resultJson,
                    SchemaVersion = validationResult.ResolvedVersion,
                    ExecutionId = null
                };
            }

            var executionId = await executionStateRepository.CreateAsync(new ExecutionStateRecord
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                IsDryRun = false,
                WasSuccessful = wasSuccessful,
                ExecutedAtUtc = DateTimeOffset.UtcNow,
                ResultJson = resultJson,
                ErrorJson = null
            }, cancellationToken);

            return new ExecuteWorkflowResultDto
            {
                IsSuccess = true,
                DryRun = false,
                Persisted = true,
                WasSuccessful = wasSuccessful,
                ResultJson = resultJson,
                SchemaVersion = validationResult.ResolvedVersion,
                ExecutionId = executionId
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
