using FluentAssertions;
using AutoMapper;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Application.Handlers;
using RulesEngine.Application.Mapping;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Tests;

public sealed class WorkflowApplicationHandlersTests
{
    private static readonly IMapper Mapper = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<WorkflowMappingProfile>();
    }).CreateMapper();

    [Fact]
    public async Task CreateWorkflowHandler_ShouldPersistWorkflowUsingRepository()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var handler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper);

        var workflow = BuildWorkflowDto("create-handler-test");

        var result = await handler.Handle(new CreateWorkflowCommand(
            Workflow: workflow,
            SchemaVersion: null),
            CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.WorkflowName.Should().Be("create-handler-test");

        repository.Store.Should().ContainSingle(item => item.Id == result.Id);
    }

    [Fact]
    public async Task ValidateWorkflowHandler_ShouldReturnErrorsForInvalidWorkflow()
    {
        var handler = new ValidateWorkflowCommandHandler(Mapper);

        var result = await handler.Handle(new ValidateWorkflowCommand(new WorkflowDto()), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateWorkflowHandler_ShouldReturnErrorsForMissingExpression()
    {
        var handler = new ValidateWorkflowCommandHandler(Mapper);
        var workflow = new WorkflowDto
        {
            WorkflowName = "validate-missing-expression",
            Rules =
            [
                new RuleDto
                {
                    RuleName = "BrokenRule",
                    Enabled = true,
                    Expression = string.Empty
                }
            ]
        };

        var result = await handler.Handle(new ValidateWorkflowCommand(workflow), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateWorkflowHandler_ShouldReturnSuccessForValidWorkflow()
    {
        var handler = new ValidateWorkflowCommandHandler(Mapper);
        var workflow = BuildWorkflowDto("validate-valid");

        var result = await handler.Handle(new ValidateWorkflowCommand(workflow), CancellationToken.None);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteWorkflowHandler_DryRun_ShouldNotPersistExecutionState()
    {
        var repository = new InMemoryWorkflowRepository();
        var executionStateRepository = new InMemoryExecutionStateRepository();
        var rulesService = new RulesEngineWorkflowService();

        var workflow = await repository.CreateAsync(new WorkflowRecord
        {
            Id = Guid.NewGuid(),
            Name = "execute-handler-test",
            Expression = "1 == 1",
            RuleJson = SerializeWorkflowDto(BuildWorkflowDto("execute-handler-test")),
            Version = 1,
            IsActive = true
        }, CancellationToken.None);

        var handler = new ExecuteWorkflowCommandHandler(repository, rulesService, executionStateRepository, Mapper);

        var result = await handler.Handle(
            new ExecuteWorkflowCommand(workflow.Id, DryRun: true, SchemaVersion: 1, Inputs: []),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.DryRun.Should().BeTrue();
        result.Persisted.Should().BeFalse();
        result.Results.Should().HaveCount(1);
        executionStateRepository.Store.Should().BeEmpty();
    }

    private static WorkflowDto BuildWorkflowDto(string workflowName) => new()
    {
        WorkflowName = workflowName,
        Rules =
        [
            new RuleDto
            {
                RuleName = "AlwaysTrue",
                Enabled = true,
                Expression = "1 == 1"
            }
        ],
        Version = 1,
        IsActive = true
    };

    private static string SerializeWorkflowDto(WorkflowDto workflow)
        => System.Text.Json.JsonSerializer.Serialize(workflow);

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
