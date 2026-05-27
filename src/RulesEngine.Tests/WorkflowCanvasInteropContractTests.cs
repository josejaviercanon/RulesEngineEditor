using FluentAssertions;

namespace RulesEngine.Tests;

public sealed class WorkflowCanvasInteropContractTests
{
    private static readonly string RepositoryRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));

    private static readonly string WorkflowCanvasInteropFile = Path.Combine(
        RepositoryRoot,
        "src",
        "RulesEngine.UI",
        "wwwroot",
        "js",
        "workflowCanvasInterop.js");

    private static readonly string EditorPageFile = Path.Combine(
        RepositoryRoot,
        "src",
        "RulesEngine.UI",
        "Pages",
        "Editor.razor");

    private static readonly string UiIndexFile = Path.Combine(
        RepositoryRoot,
        "src",
        "RulesEngine.UI",
        "wwwroot",
        "index.html");

    [Fact]
    public void WorkflowCanvasInteropModule_ShouldExposeRequiredContractMethods()
    {
        File.Exists(WorkflowCanvasInteropFile).Should().BeTrue("workflow canvas interop module must exist");

        var script = File.ReadAllText(WorkflowCanvasInteropFile);

        script.Should().Contain("window.workflowCanvasInterop");
        script.Should().Contain("initializeCanvas: function (elementId, initialData)");
        script.Should().Contain("getGraphData: function (elementId)");
        script.Should().Contain("loadGraphData: function (elementId, graphData)");
    }

    [Fact]
    public void WorkflowCanvasInteropModule_ShouldPersistAndReadGraphStateByElementId()
    {
        var script = File.ReadAllText(WorkflowCanvasInteropFile);

        script.Should().Contain("window.__workflowCanvasState = window.__workflowCanvasState || {};");
        script.Should().Contain("window.__workflowCanvasState[elementId] = state;");
        script.Should().Contain("return state ? state.graphData : null;");
        script.Should().Contain("state.graphData = graphData ?? {};");
    }

    [Fact]
    public void EditorPage_ShouldUseInteropWrapperInsteadOfDirectCanvasLibraryCalls()
    {
        File.Exists(EditorPageFile).Should().BeTrue("editor page must exist");
        var editor = File.ReadAllText(EditorPageFile);

        editor.Should().Contain("workflowCanvasInterop.initializeCanvas");
        editor.Should().Contain("workflowCanvasInterop.loadGraphData");
        editor.Should().NotContain("new LogicFlow(");
    }

    [Fact]
    public void UiIndex_ShouldRegisterWorkflowCanvasInteropScript()
    {
        File.Exists(UiIndexFile).Should().BeTrue("UI index must exist");
        var index = File.ReadAllText(UiIndexFile);

        index.Should().Contain("js/workflowCanvasInterop.js");
    }
}
