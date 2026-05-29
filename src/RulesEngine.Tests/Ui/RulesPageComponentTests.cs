using Bunit;
using Blazorise;
using Blazorise.Icons.Lucide;
using Blazorise.Tailwind;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine.UI.Pages;
using RulesEngine.UI.Services;

namespace RulesEngine.Tests.Ui;

public sealed class RulesPageComponentTests : Bunit.TestContext
{
    private readonly UiComponentTestHttpHandler _handler = new();

    public RulesPageComponentTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;

        Services.AddBlazorise().AddTailwindProviders().AddLucideIcons();
        Services.AddScoped(_ => new HttpClient(_handler)
        {
            BaseAddress = new Uri("http://localhost")
        });
        Services.AddScoped<WorkflowApiClient>();
        Services.AddScoped<WorkflowManagementService>();
    }

    [Fact]
    public void RulesComponent_ShouldExpandWorkflowRowAndRenderNestedRuleGrid()
    {
        var cut = RenderComponent<Rules>();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Sample Workflow"));

        var expandButton = cut.FindAll("button").First();
        expandButton.Click();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Sample Rule");
            _handler.GetWorkflowCalls.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void RulesComponent_NewRuleFlow_ShouldShowValidationMessageForMissingFields()
    {
        var cut = RenderComponent<Rules>();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Sample Workflow"));

        var newRuleButton = cut.FindAll("button")
            .First(button => button.TextContent.Contains("New Rule", StringComparison.Ordinal));

        newRuleButton.Click();

        var saveButton = cut.FindAll("button")
            .First(button => button.TextContent.Trim().Equals("Save", StringComparison.Ordinal));

        saveButton.Click();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Rule name and expression are required."));
    }

    [Fact]
    public void RulesComponent_EditRuleFlow_ShouldValidateAndUpdateWorkflow()
    {
        var cut = RenderComponent<Rules>();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Sample Workflow"));

        cut.FindAll("button").First().Click();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Sample Rule"));

        var editButton = cut.FindAll("button")
            .First(button => button.TextContent.Contains("Edit", StringComparison.Ordinal));

        editButton.Click();

        var saveButton = cut.FindAll("button")
            .First(button => button.TextContent.Trim().Equals("Save", StringComparison.Ordinal));

        saveButton.Click();

        cut.WaitForAssertion(() =>
        {
            _handler.ActivateRuleVersionCalls.Should().BeGreaterThan(0);
            _handler.ValidateWorkflowCalls.Should().BeGreaterThan(0);
            _handler.UpdateWorkflowCalls.Should().BeGreaterThan(0);
        });
    }
}
