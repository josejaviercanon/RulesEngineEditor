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
        loaded.Version.Should().Be(1);
        loaded.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WhenWorkflowExists_CreatesNewActiveVersionAndKeepsHistory()
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

        var versions = await repository.ListVersionsAsync(created.Id, CancellationToken.None);
        versions.Should().HaveCount(2);
        versions.Should().ContainSingle(version => version.Version == 1 && !version.IsActive);
        versions.Should().ContainSingle(version => version.Version == 2 && version.IsActive);
    }

    [Fact]
    public async Task ActivateVersionAsync_WhenOlderVersionExists_MakesItTheOnlyActiveVersion()
    {
        await using var dbContext = CreateDbContext();
        var repository = new WorkflowRepository(dbContext);
        var created = await repository.CreateAsync(CreateSampleWorkflow(), CancellationToken.None);

        await repository.UpdateAsync(
            created.Id,
            new WorkflowRecord
            {
                Name = "Updated",
                Expression = "1 == 1",
                RuleJson = "{\"WorkflowName\":\"Updated\",\"Rules\":[]}",
                Version = 2,
                IsActive = true
            },
            CancellationToken.None);

        var activated = await repository.ActivateVersionAsync(created.Id, 1, CancellationToken.None);

        activated.Should().NotBeNull();
        activated!.Version.Should().Be(1);
        activated.IsActive.Should().BeTrue();

        var versions = await repository.ListVersionsAsync(created.Id, CancellationToken.None);
        versions.Should().ContainSingle(version => version.Version == 1 && version.IsActive);
        versions.Should().ContainSingle(version => version.Version == 2 && !version.IsActive);
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
        Id = Guid.NewGuid(),
        Name = "Sample",
        Expression = "input1.value > 0",
        RuleJson = "{\"WorkflowName\":\"Sample\",\"Rules\":[]}",
        Version = 1,
        IsActive = true
    };
}
