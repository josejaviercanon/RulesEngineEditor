using FluentAssertions;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Handlers;
using RulesEngine.Application.Validation;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Tests;

public sealed class WorkflowApplicationHandlersTests
{
    [Fact]
    public async Task CreateWorkflowHandler_ShouldPersistWorkflowUsingRepository()
    {
        var repository = new InMemoryWorkflowRepository();
        var handler = new CreateWorkflowCommandHandler(repository);

        var result = await handler.Handle(new CreateWorkflowCommand(
            Name: "create-handler-test",
            Expression: "1 == 1",
            RuleJson: BuildRuleJson("create-handler-test"),
            Version: 1,
            IsActive: true,
            EffectiveFromUtc: null,
            EffectiveToUtc: null),
            CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("create-handler-test");

        repository.Store.Should().ContainSingle(item => item.Id == result.Id);
    }

    [Fact]
    public async Task ValidateWorkflowHandler_ShouldReturnResolvedVersionAndErrors()
    {
        var handler = new ValidateWorkflowCommandHandler(new JsonWorkflowSchemaValidator());

        var result = await handler.Handle(new ValidateWorkflowCommand("{not-json}", 2), CancellationToken.None);

        result.ResolvedVersion.Should().Be(2);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("valid JSON", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task ExecuteWorkflowHandler_DryRun_ShouldNotPersistExecutionState()
    {
        var repository = new InMemoryWorkflowRepository();
        var executionStateRepository = new InMemoryExecutionStateRepository();
        var rulesService = new RulesEngineWorkflowService();
        var validator = new JsonWorkflowSchemaValidator();

        var workflow = await repository.CreateAsync(new WorkflowRecord
        {
            Id = Guid.NewGuid(),
            Name = "execute-handler-test",
            Expression = "1 == 1",
            RuleJson = BuildRuleJson("execute-handler-test"),
            Version = 1,
            IsActive = true
        }, CancellationToken.None);

        var handler = new ExecuteWorkflowCommandHandler(repository, validator, rulesService, executionStateRepository);

        var result = await handler.Handle(
            new ExecuteWorkflowCommand(workflow.Id, DryRun: true, SchemaVersion: 1),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.DryRun.Should().BeTrue();
        result.Persisted.Should().BeFalse();
        executionStateRepository.Store.Should().BeEmpty();
    }

    private static string BuildRuleJson(string workflowName) => $$"""
        {
          "WorkflowName": "{{workflowName}}",
          "Rules": [
            {
              "RuleName": "AlwaysTrue",
              "Expression": "1 == 1"
            }
          ]
        }
        """;

    private sealed class InMemoryWorkflowRepository : IWorkflowRepository
    {
        public List<WorkflowRecord> Store { get; } = new();

        public Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<WorkflowRecord>>(Store.ToArray());

        public Task<WorkflowRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(Store.FirstOrDefault(item => item.Id == id));

        public Task<WorkflowRecord> CreateAsync(WorkflowRecord workflow, CancellationToken cancellationToken)
        {
            Store.Add(workflow);
            return Task.FromResult(workflow);
        }

        public Task<WorkflowRecord?> UpdateAsync(Guid id, WorkflowRecord workflow, CancellationToken cancellationToken)
        {
            var current = Store.FirstOrDefault(item => item.Id == id);
            if (current is null)
            {
                return Task.FromResult<WorkflowRecord?>(null);
            }

            current.Name = workflow.Name;
            current.Expression = workflow.Expression;
            current.RuleJson = workflow.RuleJson;
            current.Version = workflow.Version;
            current.IsActive = workflow.IsActive;
            current.EffectiveFromUtc = workflow.EffectiveFromUtc;
            current.EffectiveToUtc = workflow.EffectiveToUtc;

            return Task.FromResult<WorkflowRecord?>(current);
        }

        public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
        {
            var current = Store.FirstOrDefault(item => item.Id == id);
            if (current is null)
            {
                return Task.FromResult(false);
            }

            Store.Remove(current);
            return Task.FromResult(true);
        }
    }

    private sealed class InMemoryExecutionStateRepository : IExecutionStateRepository
    {
        public List<ExecutionStateRecord> Store { get; } = new();

        public Task<Guid> CreateAsync(ExecutionStateRecord record, CancellationToken cancellationToken)
        {
            Store.Add(record);
            return Task.FromResult(record.Id);
        }
    }
}
