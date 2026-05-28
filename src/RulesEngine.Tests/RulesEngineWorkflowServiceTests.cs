using FluentAssertions;
using RulesEngine.Core.Execution;
using RulesEngine.Models;

namespace RulesEngine.Tests;

public sealed class RulesEngineWorkflowServiceTests
{
    [Fact]
  public async Task ExecuteWorkflowAsync_WhenWorkflowIsRegistered_ReturnsSuccessfulResult()
    {
        var service = new RulesEngineWorkflowService();
    var workflowId = Guid.NewGuid();
    var workflow = new Workflow
    {
      WorkflowName = "sample-workflow",
      Rules =
      [
        new Rule
        {
          RuleName = "always-true",
          Expression = "1 == 1"
        }
      ]
    };

    var results = await service.ExecuteWorkflowAsync(workflowId, workflow, []);

    results.Should().HaveCount(1);
    results[0].IsSuccess.Should().BeTrue();
    }

    [Fact]
  public async Task ExecuteWorkflowAsync_AfterRefresh_UsesUpdatedDefinition()
    {
        var service = new RulesEngineWorkflowService();
    var workflowId = Guid.NewGuid();
    var firstWorkflow = new Workflow
    {
      WorkflowName = "refresh-workflow",
      Rules =
      [
        new Rule
        {
          RuleName = "initial",
          Expression = "1 == 1"
        }
      ]
    };
    var updatedWorkflow = new Workflow
    {
      WorkflowName = "refresh-workflow",
      Rules =
      [
        new Rule
        {
          RuleName = "updated",
          Expression = "1 == 1"
        }
      ]
    };

    var initial = await service.ExecuteWorkflowAsync(workflowId, firstWorkflow, []);
    service.RefreshWorkflow(workflowId, updatedWorkflow);
    var refreshed = await service.ExecuteWorkflowAsync(workflowId, updatedWorkflow, []);

    initial[0].Rule!.RuleName.Should().Be("initial");
    refreshed[0].Rule!.RuleName.Should().Be("updated");
    }
}
