using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RulesEngine.Core.Models;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Infrastructure.Repositories;

namespace RulesEngine.Tests;

public sealed class WorkflowRepositoryTests
{
    [Fact]
    public async Task CreateAndGetByIdAsync_RoundTripsWorkflow()
    {
        await using var dbContext = CreateDbContext();
        var repository = new WorkflowRepository(dbContext);
        var expected = CreateSampleWorkflow();

        var created = await repository.CreateAsync(expected, CancellationToken.None);
        var loaded = await repository.GetByIdAsync(created.Id, CancellationToken.None);

        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be(expected.Name);
        loaded.Expression.Should().Be(expected.Expression);
        loaded.RuleJson.Should().Be(expected.RuleJson);
        loaded.Version.Should().Be(expected.Version);
    }

    [Fact]
    public async Task UpdateAsync_WhenWorkflowExists_UpdatesPersistedValues()
    {
        await using var dbContext = CreateDbContext();
        var repository = new WorkflowRepository(dbContext);
        var created = await repository.CreateAsync(CreateSampleWorkflow(), CancellationToken.None);

        var updated = await repository.UpdateAsync(
            created.Id,
            new WorkflowRecord
            {
                Name = "Updated",
                Expression = "1 == 1",
                RuleJson = "{\"WorkflowName\":\"Updated\",\"Rules\":[]}",
                Version = 2,
                IsActive = false
            },
            CancellationToken.None);

        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated");
        updated.Version.Should().Be(2);
        updated.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_WhenWorkflowExists_RemovesWorkflow()
    {
        await using var dbContext = CreateDbContext();
        var repository = new WorkflowRepository(dbContext);
        var created = await repository.CreateAsync(CreateSampleWorkflow(), CancellationToken.None);

        var deleted = await repository.DeleteAsync(created.Id, CancellationToken.None);
        var loaded = await repository.GetByIdAsync(created.Id, CancellationToken.None);

        deleted.Should().BeTrue();
        loaded.Should().BeNull();
    }

    private static RulesEngineEditorDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<RulesEngineEditorDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new RulesEngineEditorDbContext(options);
    }

    private static WorkflowRecord CreateSampleWorkflow() => new()
    {
        Name = "Sample",
        Expression = "input1.value > 0",
        RuleJson = "{\"WorkflowName\":\"Sample\",\"Rules\":[]}",
        Version = 1,
        IsActive = true
    };
}
