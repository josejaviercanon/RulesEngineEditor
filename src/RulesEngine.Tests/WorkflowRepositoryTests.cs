using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using RulesEngine.Core.Models;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Infrastructure.Repositories;
using System.Text.Json;

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
        loaded.WorkflowJson.Should().Be(expected.WorkflowJson);
        loaded.RuleJson.Should().Be(expected.RuleJson);
        loaded.Version.Should().Be(1);
        loaded.IsActive.Should().BeTrue();
        loaded.IsEnabled.Should().BeTrue();
        loaded.Comments.Should().Be("initial");
    }

    [Fact]
    public async Task UpdateAsync_WhenWorkflowExists_UpdatesSelectedVersionInPlace()
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
                Version = 1,
                IsActive = false
            },
            CancellationToken.None);

        updated.Should().NotBeNull();
        updated!.Name.Should().Be("Updated");
        updated.Version.Should().Be(1);
        updated.WorkflowJson.Should().Be("{\"WorkflowName\":\"Updated\",\"Rules\":[]}");

        var versions = await repository.ListVersionsAsync(created.Id, CancellationToken.None);
        versions.Should().HaveCount(1);
        versions.Should().ContainSingle(version => version.Version == 1 && version.IsActive);
        versions.Should().ContainSingle(version => version.Version == 1 && version.IsEnabled);
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
                Version = 1,
                IsActive = true
            },
            CancellationToken.None);

        await repository.CreateAsync(new WorkflowRecord
        {
            Id = created.Id,
            Name = "Updated-V2",
            Expression = "1 == 1",
            RuleJson = "{\"WorkflowName\":\"Updated-V2\",\"Rules\":[]}",
            IsActive = true,
            IsEnabled = true
        }, CancellationToken.None);

        var activated = await repository.ActivateVersionAsync(created.Id, 1, CancellationToken.None);

        activated.Should().NotBeNull();
        activated!.Version.Should().Be(1);
        activated.IsActive.Should().BeTrue();

        var versions = await repository.ListVersionsAsync(created.Id, CancellationToken.None);
        versions.Should().ContainSingle(version => version.Version == 1 && version.IsActive);
        versions.Should().ContainSingle(version => version.Version == 2 && !version.IsActive);
    }

    [Fact]
    public async Task SetVersionEnabledAsync_WhenVersionIsActive_UpdatesEnablement()
    {
        await using var dbContext = CreateDbContext();
        var repository = new WorkflowRepository(dbContext);
        var created = await repository.CreateAsync(CreateSampleWorkflow(), CancellationToken.None);

        var disabled = await repository.SetVersionEnabledAsync(created.Id, created.Version, false, CancellationToken.None);
        disabled.Should().NotBeNull();
        disabled!.IsEnabled.Should().BeFalse();

        var loaded = await repository.GetByIdAsync(created.Id, CancellationToken.None);
        loaded.Should().NotBeNull();
        loaded!.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task ListAsync_WhenFilteredByEnablement_ReturnsExpectedItems()
    {
        await using var dbContext = CreateDbContext();
        var repository = new WorkflowRepository(dbContext);

        var enabled = await repository.CreateAsync(CreateSampleWorkflow(), CancellationToken.None);
        var disabled = await repository.CreateAsync(new WorkflowRecord
        {
            Id = Guid.NewGuid(),
            Name = "DisabledSample",
            Expression = "input1.value > 10",
            RuleJson = "{\"WorkflowName\":\"DisabledSample\",\"Rules\":[]}",
            Version = 1,
            IsActive = true,
            IsEnabled = false,
            Comments = "disabled"
        }, CancellationToken.None);

        var onlyEnabled = await repository.ListAsync(CancellationToken.None, WorkflowRuleQueryMode.ActiveOnly, true);
        onlyEnabled.Should().ContainSingle(item => item.Id == enabled.Id);
        onlyEnabled.Should().NotContain(item => item.Id == disabled.Id);

        var onlyDisabled = await repository.ListAsync(CancellationToken.None, WorkflowRuleQueryMode.ActiveOnly, false);
        onlyDisabled.Should().ContainSingle(item => item.Id == disabled.Id);
        onlyDisabled.Should().NotContain(item => item.Id == enabled.Id);
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

    [Fact]
    public async Task ListWorkflowRulesAsync_ShouldOrderByExecuteOrderThenRuleGuidId()
    {
        await using var dbContext = CreateDbContext();
        var repository = new WorkflowRepository(dbContext);
        var firstRuleGuid = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var secondRuleGuid = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var thirdRuleGuid = Guid.Parse("33333333-3333-3333-3333-333333333333");

        var workflowJson = JsonSerializer.Serialize(new
        {
            WorkflowName = "ordered-rules",
            Rules = new object[]
            {
                new { RuleGuidId = thirdRuleGuid, RuleName = "Third", Expression = "1 == 1", ExecuteOrder = 2, Version = 1, IsActive = true },
                new { RuleGuidId = secondRuleGuid, RuleName = "Second", Expression = "1 == 1", ExecuteOrder = 1, Version = 1, IsActive = true },
                new { RuleGuidId = firstRuleGuid, RuleName = "First", Expression = "1 == 1", ExecuteOrder = 1, Version = 1, IsActive = true }
            }
        });

        var created = await repository.CreateAsync(new WorkflowRecord
        {
            Id = Guid.NewGuid(),
            Name = "ordered-rules",
            Expression = "1 == 1",
            WorkflowJson = workflowJson,
            RuleJson = workflowJson,
            IsActive = true,
            IsEnabled = true
        }, CancellationToken.None);

        var rules = await repository.ListWorkflowRulesAsync(
            created.Id,
            created.Version,
            WorkflowRuleQueryMode.ActiveOnly,
            CancellationToken.None);

        rules.Select(rule => rule.RuleGuidId)
            .Should().Equal(firstRuleGuid, secondRuleGuid, thirdRuleGuid);
        rules.Select(rule => rule.ExecuteOrder)
            .Should().Equal(1, 1, 2);
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
        WorkflowJson = "{\"WorkflowName\":\"Sample\",\"Rules\":[]}",
        RuleJson = "{\"WorkflowName\":\"Sample\",\"Rules\":[]}",
        Version = 1,
        IsActive = true,
        IsEnabled = true,
        Comments = "initial"
    };
}
