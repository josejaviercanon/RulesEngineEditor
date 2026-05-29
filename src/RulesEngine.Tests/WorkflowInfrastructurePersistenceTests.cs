using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RulesEngine.Core.Models;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Infrastructure.Repositories;

namespace RulesEngine.Tests;

public sealed class WorkflowInfrastructurePersistenceTests
{
    [Fact]
    public async Task WorkflowRepository_ShouldPersistAndLoadWorkflowDefinition()
    {
        await using var dbContext = CreateDbContext();
        var repository = new WorkflowRepository(dbContext);

        var created = await repository.CreateAsync(new WorkflowRecord
        {
            Id = Guid.NewGuid(),
            Name = "infra-persist-workflow",
            Expression = "1 == 1",
            RuleJson = "{}",
            Version = 1,
            IsActive = true,
            IsEnabled = false,
            Comments = "infra test"
        }, CancellationToken.None);

        var loaded = await repository.GetByIdAsync(created.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("infra-persist-workflow");
        loaded.Expression.Should().Be("1 == 1");
        loaded.RuleJson.Should().Be("{}");
        loaded.Version.Should().Be(1);
        loaded.IsActive.Should().BeTrue();
        loaded.IsEnabled.Should().BeFalse();
        loaded.Comments.Should().Be("infra test");
    }

    [Fact]
    public async Task WorkflowRepository_ShouldRoundTripCommentsAt4000CharacterBoundary()
    {
        await using var dbContext = CreateDbContext();
        var repository = new WorkflowRepository(dbContext);
        var comments = new string('c', 4000);

        var created = await repository.CreateAsync(new WorkflowRecord
        {
            Id = Guid.NewGuid(),
            Name = "infra-comment-boundary",
            Expression = "1 == 1",
            RuleJson = "{}",
            Version = 1,
            IsActive = true,
            IsEnabled = true,
            Comments = comments
        }, CancellationToken.None);

        var loaded = await repository.GetByIdAsync(created.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Comments.Should().Be(comments);
    }

    [Fact]
    public async Task ExecutionStateRepository_ShouldPersistExecutionStateRecord()
    {
        await using var dbContext = CreateDbContext();
        var repository = new ExecutionStateRepository(dbContext);
        var record = new ExecutionStateRecord
        {
            Id = Guid.NewGuid(),
            WorkflowId = Guid.NewGuid(),
            IsDryRun = false,
            WasSuccessful = true,
            ExecutedAtUtc = DateTimeOffset.UtcNow,
            ResultJson = "[]"
        };

        var persistedId = await repository.CreateAsync(record, CancellationToken.None);

        var persisted = await dbContext.ExecutionStates.AsNoTracking().FirstOrDefaultAsync(state => state.Id == persistedId);
        persisted.Should().NotBeNull();
        persisted!.WorkflowId.Should().Be(record.WorkflowId);
        persisted.ResultJson.Should().Be("[]");
    }

    private static RulesEngineEditorDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RulesEngineEditorDbContext>()
            .UseInMemoryDatabase($"workflow-infra-tests-{Guid.NewGuid()}")
            .Options;

        var context = new RulesEngineEditorDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}
