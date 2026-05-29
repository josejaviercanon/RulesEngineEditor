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
        result.Version.Should().Be(1);
        result.IsActive.Should().BeTrue();

        repository.Store.Should().ContainSingle(item => item.Id == result.Id);
    }

    [Fact]
    public async Task UpdateWorkflowHandler_ShouldCreateNewVersionInsteadOfOverwritingHistory()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var createHandler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper);
        var updateHandler = new UpdateWorkflowCommandHandler(repository, rulesService, Mapper);

        var created = await createHandler.Handle(new CreateWorkflowCommand(BuildWorkflowDto("workflow-v1"), null), CancellationToken.None);

        var updated = await updateHandler.Handle(
            new UpdateWorkflowCommand(created.Id, BuildWorkflowDto("workflow-v2"), null),
            CancellationToken.None);

        updated.Should().NotBeNull();
        updated!.Version.Should().Be(2);
        updated.IsActive.Should().BeTrue();
        repository.Store.Should().HaveCount(2);
        repository.Store.Should().ContainSingle(item => item.Version == 1 && !item.IsActive);
        repository.Store.Should().ContainSingle(item => item.Version == 2 && item.IsActive);
    }

    [Fact]
    public async Task ActivateWorkflowVersionHandler_ShouldSwitchActiveVersion()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var createHandler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper);
        var updateHandler = new UpdateWorkflowCommandHandler(repository, rulesService, Mapper);
        var activateHandler = new ActivateWorkflowVersionCommandHandler(repository, rulesService, Mapper);

        var created = await createHandler.Handle(new CreateWorkflowCommand(BuildWorkflowDto("workflow-v1"), null), CancellationToken.None);
        await updateHandler.Handle(new UpdateWorkflowCommand(created.Id, BuildWorkflowDto("workflow-v2"), null), CancellationToken.None);

        var activated = await activateHandler.Handle(new ActivateWorkflowVersionCommand(created.Id, 1), CancellationToken.None);

        activated.Should().NotBeNull();
        activated!.Version.Should().Be(1);
        activated.IsActive.Should().BeTrue();
        repository.Store.Should().ContainSingle(item => item.Version == 1 && item.IsActive);
        repository.Store.Should().ContainSingle(item => item.Version == 2 && !item.IsActive);
    }

    [Fact]
    public async Task GetWorkflowVersionHandler_ShouldReturnRequestedVersion()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var createHandler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper);
        var updateHandler = new UpdateWorkflowCommandHandler(repository, rulesService, Mapper);
        var getVersionHandler = new GetWorkflowVersionQueryHandler(repository);

        var created = await createHandler.Handle(new CreateWorkflowCommand(BuildWorkflowDto("workflow-v1"), null), CancellationToken.None);
        await updateHandler.Handle(new UpdateWorkflowCommand(created.Id, BuildWorkflowDto("workflow-v2"), null), CancellationToken.None);

        var versionOne = await getVersionHandler.Handle(new GetWorkflowVersionQuery(created.Id, 1), CancellationToken.None);
        var versionTwo = await getVersionHandler.Handle(new GetWorkflowVersionQuery(created.Id, 2), CancellationToken.None);

        versionOne.Should().NotBeNull();
        versionOne!.Version.Should().Be(1);
        versionOne.IsActive.Should().BeFalse();
        versionOne.WorkflowName.Should().Be("workflow-v1");

        versionTwo.Should().NotBeNull();
        versionTwo!.Version.Should().Be(2);
        versionTwo.IsActive.Should().BeTrue();
        versionTwo.WorkflowName.Should().Be("workflow-v2");
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
            => Task.FromResult<IReadOnlyCollection<WorkflowRecord>>(Store
                .GroupBy(item => item.Id)
                .Select(group => group.OrderByDescending(item => item.IsActive).ThenByDescending(item => item.Version).First())
                .ToArray());

        public Task<IReadOnlyCollection<WorkflowRecord>> ListVersionsAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<WorkflowRecord>>(Store
                .Where(item => item.Id == id)
                .OrderBy(item => item.Version)
                .ToArray());

        public Task<WorkflowRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
            => Task.FromResult(Store
                .Where(item => item.Id == id)
                .OrderByDescending(item => item.IsActive)
                .ThenByDescending(item => item.Version)
                .FirstOrDefault());

        public Task<WorkflowRecord?> GetVersionAsync(Guid id, int version, CancellationToken cancellationToken)
            => Task.FromResult(Store.FirstOrDefault(item => item.Id == id && item.Version == version));

        public Task<WorkflowRecord?> ActivateVersionAsync(Guid id, int version, CancellationToken cancellationToken)
        {
            var items = Store.Where(item => item.Id == id).ToList();
            var target = items.FirstOrDefault(item => item.Version == version);
            if (target is null)
            {
                return Task.FromResult<WorkflowRecord?>(null);
            }

            foreach (var item in items)
            {
                item.IsActive = item.Version == version;
            }

            return Task.FromResult<WorkflowRecord?>(target);
        }

        public Task<WorkflowRecord> CreateAsync(WorkflowRecord workflow, CancellationToken cancellationToken)
        {
            var identity = workflow.Id == Guid.Empty ? Guid.NewGuid() : workflow.Id;
            var version = Store.Where(item => item.Id == identity).Select(item => item.Version).DefaultIfEmpty(0).Max() + 1;

            foreach (var item in Store.Where(item => item.Id == identity && item.IsActive))
            {
                item.IsActive = false;
            }

            var created = new WorkflowRecord
            {
                Id = identity,
                Name = workflow.Name,
                Expression = workflow.Expression,
                RuleJson = workflow.RuleJson,
                Version = version,
                IsActive = true,
                EffectiveFromUtc = workflow.EffectiveFromUtc,
                EffectiveToUtc = workflow.EffectiveToUtc
            };

            Store.Add(created);
            return Task.FromResult(created);
        }

        public Task<WorkflowRecord?> UpdateAsync(Guid id, WorkflowRecord workflow, CancellationToken cancellationToken)
        {
            var items = Store.Where(item => item.Id == id).ToList();
            if (items.Count == 0)
            {
                return Task.FromResult<WorkflowRecord?>(null);
            }

            foreach (var item in items.Where(item => item.IsActive))
            {
                item.IsActive = false;
            }

            var updated = new WorkflowRecord
            {
                Id = id,
                Name = workflow.Name,
                Expression = workflow.Expression,
                RuleJson = workflow.RuleJson,
                Version = items.Max(item => item.Version) + 1,
                IsActive = true,
                EffectiveFromUtc = workflow.EffectiveFromUtc,
                EffectiveToUtc = workflow.EffectiveToUtc
            };

            Store.Add(updated);
            return Task.FromResult<WorkflowRecord?>(updated);
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
