using FluentAssertions;
using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Application.Handlers;
using RulesEngine.Application.Mapping;
using RulesEngine.Application.Policies;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;
using System.Text.Json;

namespace RulesEngine.Tests;

public sealed class WorkflowApplicationHandlersTests
{
    private static readonly RuleStatusPolicy StatusPolicy = new();

    private static readonly IMapper Mapper = new MapperConfiguration(cfg =>
    {
        cfg.AddProfile<WorkflowMappingProfile>();
    }, NullLoggerFactory.Instance).CreateMapper();

    [Fact]
    public async Task CreateWorkflowHandler_ShouldPersistWorkflowUsingRepository()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var handler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);

        var workflow = BuildWorkflowDto("create-handler-test");

        var result = await handler.Handle(new CreateWorkflowCommand(
            Workflow: workflow,
            SchemaVersion: null),
            CancellationToken.None);

        result.Id.Should().NotBeEmpty();
        result.WorkflowName.Should().Be("create-handler-test");
        result.Version.Should().Be(1);
        result.IsActive.Should().BeTrue();
        result.WorkflowJson.Should().Contain("\"WorkflowName\":\"create-handler-test\"");
        result.Rules.Should().ContainSingle();
        result.Rules[0].RuleJson.Should().Contain("\"Expression\":\"1 == 1\"");

        repository.Store.Should().ContainSingle(item => item.Id == result.Id);
    }

    [Fact]
    public async Task UpdateWorkflowHandler_ShouldUpdateSelectedVersionInPlace()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var createHandler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);
        var updateHandler = new UpdateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);

        var created = await createHandler.Handle(new CreateWorkflowCommand(BuildWorkflowDto("workflow-v1"), null), CancellationToken.None);

        var updated = await updateHandler.Handle(
            new UpdateWorkflowCommand(created.Id, BuildWorkflowDto("workflow-v2"), null),
            CancellationToken.None);

        updated.Should().NotBeNull();
        updated!.Version.Should().Be(1);
        updated.IsActive.Should().BeTrue();
        repository.Store.Should().HaveCount(1);
        repository.Store.Should().ContainSingle(item => item.Version == 1 && item.IsActive && item.Name == "workflow-v2");
    }

    [Fact]
    public async Task UpdateWorkflowHandler_ShouldCreateNewVersion_WhenCreateVersionIntentIsTrue()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var createHandler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);
        var updateHandler = new UpdateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);

        var created = await createHandler.Handle(new CreateWorkflowCommand(BuildWorkflowDto("workflow-v1"), null), CancellationToken.None);

        var updated = await updateHandler.Handle(
            new UpdateWorkflowCommand(created.Id, BuildWorkflowDto("workflow-v2"), null, CreateNewVersion: true),
            CancellationToken.None);

        updated.Should().NotBeNull();
        updated!.Version.Should().Be(2);
        repository.Store.Should().HaveCount(2);
        repository.Store.Should().ContainSingle(item => item.Version == 1 && !item.IsActive);
        repository.Store.Should().ContainSingle(item => item.Version == 2 && item.IsActive);
    }

    [Fact]
    public async Task ActivateWorkflowVersionHandler_ShouldSwitchActiveVersion()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var createHandler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);
        var updateHandler = new UpdateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);
        var activateHandler = new ActivateWorkflowVersionCommandHandler(repository, rulesService, Mapper);

        var created = await createHandler.Handle(new CreateWorkflowCommand(BuildWorkflowDto("workflow-v1"), null), CancellationToken.None);
        await updateHandler.Handle(new UpdateWorkflowCommand(created.Id, BuildWorkflowDto("workflow-v2"), null, CreateNewVersion: true), CancellationToken.None);

        var activated = await activateHandler.Handle(new ActivateWorkflowVersionCommand(created.Id, 1), CancellationToken.None);

        activated.Should().NotBeNull();
        activated!.Version.Should().Be(1);
        activated.IsActive.Should().BeTrue();
        repository.Store.Should().ContainSingle(item => item.Version == 1 && item.IsActive);
        repository.Store.Should().ContainSingle(item => item.Version == 2 && !item.IsActive);
    }

    [Fact]
    public async Task SetWorkflowVersionEnabledHandler_ShouldRejectNonActiveVersion()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var createHandler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);
        var updateHandler = new UpdateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);
        var setEnabledHandler = new SetWorkflowVersionEnabledCommandHandler(repository);

        var created = await createHandler.Handle(new CreateWorkflowCommand(BuildWorkflowDto("workflow-v1"), null), CancellationToken.None);
        await updateHandler.Handle(new UpdateWorkflowCommand(created.Id, BuildWorkflowDto("workflow-v2"), null, CreateNewVersion: true), CancellationToken.None);

        var action = async () => await setEnabledHandler.Handle(
            new SetWorkflowVersionEnabledCommand(created.Id, 1, false),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Only the active workflow version can be enabled or disabled.*");
    }

    [Fact]
    public async Task CreateWorkflowHandler_ShouldRejectCommentsLongerThan4000()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var handler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);

        var workflow = BuildWorkflowDto("comment-length");
        workflow = new WorkflowDto
        {
            Id = workflow.Id,
            WorkflowName = workflow.WorkflowName,
            RuleExpressionType = workflow.RuleExpressionType,
            GlobalParams = workflow.GlobalParams,
            Rules = workflow.Rules,
            WorkflowsToInject = workflow.WorkflowsToInject,
            Version = workflow.Version,
            IsActive = workflow.IsActive,
            IsEnabled = workflow.IsEnabled,
            Comments = new string('x', 4001)
        };

        var action = async () => await handler.Handle(
            new CreateWorkflowCommand(workflow, null),
            CancellationToken.None);

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Comments cannot exceed 4000 characters.");
    }

    [Fact]
    public async Task GetWorkflowVersionHandler_ShouldReturnRequestedVersion()
    {
        var repository = new InMemoryWorkflowRepository();
        var rulesService = new RulesEngineWorkflowService();
        var createHandler = new CreateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);
        var updateHandler = new UpdateWorkflowCommandHandler(repository, rulesService, Mapper, StatusPolicy);
        var getVersionHandler = new GetWorkflowVersionQueryHandler(repository);

        var created = await createHandler.Handle(new CreateWorkflowCommand(BuildWorkflowDto("workflow-v1"), null), CancellationToken.None);
        await updateHandler.Handle(new UpdateWorkflowCommand(created.Id, BuildWorkflowDto("workflow-v2"), null, CreateNewVersion: true), CancellationToken.None);

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
        var handler = new ValidateWorkflowCommandHandler(Mapper, StatusPolicy);

        var result = await handler.Handle(new ValidateWorkflowCommand(new WorkflowDto()), CancellationToken.None);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateWorkflowHandler_ShouldReturnErrorsForMissingExpression()
    {
        var handler = new ValidateWorkflowCommandHandler(Mapper, StatusPolicy);
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
        var handler = new ValidateWorkflowCommandHandler(Mapper, StatusPolicy);
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
            WorkflowJson = SerializeWorkflowDto(BuildWorkflowDto("execute-handler-test")),
            RuleJson = SerializeWorkflowDto(BuildWorkflowDto("execute-handler-test")),
            Version = 1,
            IsActive = true
        }, CancellationToken.None);

        var handler = new ExecuteWorkflowCommandHandler(repository, rulesService, executionStateRepository, Mapper, StatusPolicy);

        var result = await handler.Handle(
            new ExecuteWorkflowCommand(workflow.Id, DryRun: true, SchemaVersion: 1, Inputs: [], IncludeStatuses: null),
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
        IsActive = true,
        IsEnabled = true,
        Comments = "handler test"
    };

    private static string SerializeWorkflowDto(WorkflowDto workflow)
        => System.Text.Json.JsonSerializer.Serialize(workflow);

    private sealed class InMemoryWorkflowRepository : IWorkflowRepository
    {
        public List<WorkflowRecord> Store { get; } = new();

        public Task<IReadOnlyCollection<WorkflowRecord>> ListAsync(
            CancellationToken cancellationToken,
            WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly,
            bool? isEnabled = null)
            => Task.FromResult<IReadOnlyCollection<WorkflowRecord>>(Store
                .GroupBy(item => item.Id)
                .Select(group => group.OrderByDescending(item => item.IsActive).ThenByDescending(item => item.Version).First())
                .Where(item => !isEnabled.HasValue || item.IsEnabled == isEnabled.Value)
                .ToArray());

        public Task<IReadOnlyCollection<WorkflowRecord>> ListVersionsAsync(
            Guid id,
            CancellationToken cancellationToken,
            WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly,
            bool? isEnabled = null)
            => Task.FromResult<IReadOnlyCollection<WorkflowRecord>>(Store
                .Where(item => item.Id == id)
                .Where(item => !isEnabled.HasValue || item.IsEnabled == isEnabled.Value)
                .OrderBy(item => item.Version)
                .ToArray());

        public Task<WorkflowRecord?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken,
            WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly,
            bool? isEnabled = null)
            => Task.FromResult(Store
                .Where(item => item.Id == id)
                .OrderByDescending(item => item.IsActive)
                .ThenByDescending(item => item.Version)
                .FirstOrDefault(item => !isEnabled.HasValue || item.IsEnabled == isEnabled.Value));

        public Task<WorkflowRecord?> GetVersionAsync(
            Guid id,
            int version,
            CancellationToken cancellationToken,
            WorkflowRuleQueryMode ruleQueryMode = WorkflowRuleQueryMode.ActiveOnly,
            bool? isEnabled = null)
            => Task.FromResult(Store.FirstOrDefault(item =>
                item.Id == id &&
                item.Version == version &&
                (!isEnabled.HasValue || item.IsEnabled == isEnabled.Value)));

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

        public Task<WorkflowRecord?> SetVersionEnabledAsync(Guid id, int version, bool isEnabled, CancellationToken cancellationToken)
        {
            var items = Store.Where(item => item.Id == id).ToList();
            var target = items.FirstOrDefault(item => item.Version == version);
            if (target is null)
            {
                return Task.FromResult<WorkflowRecord?>(null);
            }

            target.IsEnabled = isEnabled;
            if (isEnabled)
            {
                foreach (var item in items.Where(item => item.Version != version && item.IsActive))
                {
                    item.IsEnabled = false;
                }
            }

            return Task.FromResult<WorkflowRecord?>(target);
        }

        public Task<IReadOnlyCollection<RuleVersionRecord>> ListRuleVersionsAsync(Guid workflowId, Guid ruleGuidId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyCollection<RuleVersionRecord>>([]);

        public Task<RuleVersionRecord?> ActivateRuleVersionAsync(Guid workflowId, Guid ruleGuidId, int version, CancellationToken cancellationToken)
            => Task.FromResult<RuleVersionRecord?>(null);

        public Task<IReadOnlyCollection<RuleVersionRecord>> ListWorkflowRulesAsync(
            Guid workflowId,
            int workflowVersion,
            WorkflowRuleQueryMode mode,
            CancellationToken cancellationToken)
        {
            var workflow = Store.FirstOrDefault(item => item.Id == workflowId && item.Version == workflowVersion);
            if (workflow is null)
            {
                return Task.FromResult<IReadOnlyCollection<RuleVersionRecord>>([]);
            }

            var workflowJson = string.IsNullOrWhiteSpace(workflow.WorkflowJson)
                ? workflow.RuleJson
                : workflow.WorkflowJson;
            var dto = JsonSerializer.Deserialize<WorkflowDto>(workflowJson);
            if (dto is null)
            {
                return Task.FromResult<IReadOnlyCollection<RuleVersionRecord>>([]);
            }

            var rules = dto.Rules.Select(rule => new RuleVersionRecord
            {
                Id = Guid.NewGuid(),
                RuleGuidId = rule.RuleGuidId == Guid.Empty ? Guid.NewGuid() : rule.RuleGuidId,
                Name = rule.RuleName,
                Expression = rule.Expression,
                RuleJson = JsonSerializer.Serialize(rule),
                Version = rule.Version == 0 ? 1 : rule.Version,
                IsActive = true,
                Status = RuleStatusParser.ParseOrDefault(rule.Status)
            }).ToArray();

            return Task.FromResult<IReadOnlyCollection<RuleVersionRecord>>(rules);
        }

        public Task ApplyRuleStatusUpdatesAsync(IReadOnlyCollection<RuleStatusUpdateRecord> updates, CancellationToken cancellationToken)
            => Task.CompletedTask;

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
                WorkflowJson = string.IsNullOrWhiteSpace(workflow.WorkflowJson) ? workflow.RuleJson : workflow.WorkflowJson,
                RuleJson = string.IsNullOrWhiteSpace(workflow.WorkflowJson) ? workflow.RuleJson : workflow.WorkflowJson,
                Version = version,
                IsActive = true,
                IsEnabled = workflow.IsEnabled,
                Comments = workflow.Comments,
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

            var target = workflow.Version > 0
                ? items.FirstOrDefault(item => item.Version == workflow.Version)
                : items.FirstOrDefault(item => item.IsActive);

            if (target is null)
            {
                return Task.FromResult<WorkflowRecord?>(null);
            }

            target.Name = workflow.Name;
            target.Expression = workflow.Expression;
            target.WorkflowJson = string.IsNullOrWhiteSpace(workflow.WorkflowJson) ? workflow.RuleJson : workflow.WorkflowJson;
            target.RuleJson = target.WorkflowJson;
            target.IsEnabled = workflow.IsEnabled;
            target.Comments = workflow.Comments;
            target.EffectiveFromUtc = workflow.EffectiveFromUtc;
            target.EffectiveToUtc = workflow.EffectiveToUtc;

            return Task.FromResult<WorkflowRecord?>(target);
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
