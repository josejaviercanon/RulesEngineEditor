using Bunit;
using Blazorise;
using Blazorise.Icons.Lucide;
using Blazorise.Tailwind;
using FluentAssertions;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine.UI.Pages;
using RulesEngine.UI.Services;

namespace RulesEngine.Tests.Ui;

public sealed class HomePageComponentTests : Bunit.TestContext
{
    private readonly UiComponentTestHttpHandler _handler = new();

    public HomePageComponentTests()
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
    public void HomeComponent_ShouldDeclareDefaultRoute()
    {
        var routeAttributes = typeof(Home).GetCustomAttributes(typeof(RouteAttribute), inherit: true)
            .Cast<RouteAttribute>()
            .ToArray();

        routeAttributes.Should().Contain(attribute => attribute.Template == "/");
    }

    [Fact]
    public void HomeComponent_ShouldRenderWorkflowGridFromApi()
    {
        var cut = RenderComponent<Home>();

        cut.WaitForAssertion(() =>
        {
            cut.Markup.Should().Contain("Workflow Home");
            cut.Markup.Should().Contain("Sample Workflow");
            _handler.ListWorkflowsCalls.Should().BeGreaterThan(0);
        });
    }

    [Fact]
    public void HomeComponent_EditAction_ShouldLoadWorkflowVersions()
    {
        var cut = RenderComponent<Home>();

        cut.WaitForAssertion(() => cut.Markup.Should().Contain("Sample Workflow"));

        var editButton = cut.FindAll("button")
            .First(button => button.TextContent.Contains("Edit", StringComparison.Ordinal));

        editButton.Click();

        cut.WaitForAssertion(() => _handler.ListVersionsCalls.Should().BeGreaterThan(0));
    }
}
