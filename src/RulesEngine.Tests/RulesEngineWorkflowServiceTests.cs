using FluentAssertions;
using RulesEngine.Core.Execution;

namespace RulesEngine.Tests;

public sealed class RulesEngineWorkflowServiceTests
{
    [Fact]
    public void AddOrUpdateWorkflow_WhenJsonIsEmpty_ThrowsArgumentException()
    {
        var service = new RulesEngineWorkflowService();

        var action = () => service.AddOrUpdateWorkflow(string.Empty);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task ExecuteAllRulesAsync_WhenWorkflowIsRegistered_ReturnsSuccessfulResult()
    {
        var service = new RulesEngineWorkflowService();
        service.AddOrUpdateWorkflow("""
            {
              "WorkflowName": "sample-workflow",
              "Rules": [
                {
                  "RuleName": "always-true",
                  "Expression": "1 == 1"
                }
              ]
            }
            """);

        var results = await service.ExecuteAllRulesAsync("sample-workflow");

        results.Should().HaveCount(1);
        results[0].IsSuccess.Should().BeTrue();
    }
}
