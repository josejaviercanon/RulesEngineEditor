using FluentAssertions;
using RulesEngine.Application.Policies;
using RulesEngine.Core.Models;

namespace RulesEngine.Tests;

public sealed class RuleStatusPolicyTests
{
    private readonly RuleStatusPolicy _policy = new();

    [Theory]
    [InlineData("draft", RuleStatus.Draft)]
    [InlineData("failed", RuleStatus.Failed)]
    [InlineData("disabled", RuleStatus.Disabled)]
    [InlineData("production", RuleStatus.Production)]
    [InlineData("PRODUCTION", RuleStatus.Production)]
    public void NormalizeOrDefault_ShouldParseKnownValues(string value, RuleStatus expected)
    {
        var status = _policy.NormalizeOrDefault(value);

        status.Should().Be(expected);
    }

    [Fact]
    public void ResolveUserRequestedStatus_ShouldBlockProductionWhenValidationFails()
    {
        var status = _policy.ResolveUserRequestedStatus(
            previousStatus: RuleStatus.Draft,
            requestedStatus: "production",
            validationsPassed: false);

        status.Should().Be(RuleStatus.Draft);
    }

    [Fact]
    public void ResolveCompileFailureStatus_ShouldMoveProductionToFailed()
    {
        _policy.ResolveCompileFailureStatus(RuleStatus.Production)
            .Should().Be(RuleStatus.Failed);
    }

    [Fact]
    public void ResolveExecutionFailureStatus_ShouldMoveOnlyProductionToFailed()
    {
        _policy.ResolveExecutionFailureStatus(RuleStatus.Production).Should().Be(RuleStatus.Failed);
        _policy.ResolveExecutionFailureStatus(RuleStatus.Draft).Should().Be(RuleStatus.Draft);
        _policy.ResolveExecutionFailureStatus(RuleStatus.Failed).Should().Be(RuleStatus.Failed);
        _policy.ResolveExecutionFailureStatus(RuleStatus.Disabled).Should().Be(RuleStatus.Disabled);
    }
}
