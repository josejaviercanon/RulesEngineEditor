using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using RulesEngine.Application.Dtos;
using RulesEngine.Application.Mapping;

namespace RulesEngine.Tests;

public sealed class WorkflowDtoSerializationTests
{
    [Fact]
    public void WorkflowDto_ShouldRoundTripThroughJson()
    {
        var original = new WorkflowDto
        {
            Id = Guid.NewGuid(),
            WorkflowName = "RoundTripWorkflow",
            Rules =
            [
                new RuleDto
                {
                    RuleName = "AlwaysTrue",
                    Enabled = true,
                    Expression = "1 == 1",
                    LocalParams =
                    [
                        new ScopedParamDto
                        {
                            Name = "x",
                            Expression = "1"
                        }
                    ]
                }
            ],
            GlobalParams =
            [
                new ScopedParamDto
                {
                    Name = "g",
                    Expression = "2"
                }
            ],
            Version = 2,
            IsActive = true
        };

        var json = JsonSerializer.Serialize(original);
        var roundTrip = JsonSerializer.Deserialize<WorkflowDto>(json);

        roundTrip.Should().NotBeNull();
        roundTrip!.WorkflowName.Should().Be(original.WorkflowName);
        roundTrip.Rules.Should().HaveCount(1);
        roundTrip.Rules[0].RuleName.Should().Be("AlwaysTrue");
        roundTrip.Rules[0].LocalParams.Should().HaveCount(1);
        roundTrip.GlobalParams.Should().HaveCount(1);
        roundTrip.Version.Should().Be(2);
        roundTrip.IsActive.Should().BeTrue();
    }

    [Fact]
    public void WorkflowDto_ShouldDeserializeFromSampleFixture()
    {
        var repositoryRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
        var fixturePath = Path.Combine(
            repositoryRoot,
            "Legacy",
            "demo",
            "RulesEngineEditorWebAssembly",
            "wwwroot",
            "sample-data",
            "discount.json");

        var json = File.ReadAllText(fixturePath);
        var workflows = JsonSerializer.Deserialize<List<WorkflowDto>>(json);

        workflows.Should().NotBeNull();
        workflows!.Should().NotBeEmpty();
        workflows[0].WorkflowName.Should().Be("Discount");
        workflows[0].Rules.Should().NotBeEmpty();
        workflows[0].Rules[0].Expression.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void WorkflowMappingProfile_ShouldBeValid()
    {
        var configuration = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<WorkflowMappingProfile>();
        }, NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
