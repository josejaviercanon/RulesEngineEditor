using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using RulesEngine.API.Contracts;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine.Tests.Infrastructure;

namespace RulesEngine.Tests;

public sealed class WorkflowApiIntegrationTests : IClassFixture<WorkflowApiFactory>
{
    private readonly HttpClient _client;
    private readonly WorkflowApiFactory _factory;

    public WorkflowApiIntegrationTests(WorkflowApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task WorkflowCrudEndpoints_ShouldSupportCreateReadUpdateDelete()
    {
        const string workflowName = "CrudWorkflow";
        var createRequest = CreateWorkflowRequest(workflowName);

        var createResponse = await _client.PostAsJsonAsync("/api/workflows", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdId = ResolveWorkflowId(createResponse);

        var getByIdResponse = await _client.GetAsync($"/api/workflows/{createdId}");
        getByIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var loaded = await getByIdResponse.Content.ReadFromJsonAsync<WorkflowResponse>();
        loaded.Should().NotBeNull();
        loaded!.Name.Should().Be("CrudWorkflow");

        var updateRequest = createRequest with
        {
            Name = "CrudWorkflowUpdated",
            Expression = "1 == 1",
            RuleJson = BuildRuleJson("CrudWorkflowUpdated"),
            Version = 2,
            IsActive = false
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/workflows/{createdId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var listResponse = await _client.GetAsync("/api/workflows");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var workflows = await listResponse.Content.ReadFromJsonAsync<List<WorkflowResponse>>();
        workflows.Should().NotBeNull();
        workflows!.Should().Contain(item => item.Id == createdId && item.Name == "CrudWorkflowUpdated");

        var deleteResponse = await _client.DeleteAsync($"/api/workflows/{createdId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/workflows/{createdId}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ExecuteEndpoint_ShouldSupportDryRunAndPersistedExecution()
    {
        const string workflowName = "ExecuteWorkflow";
        var createResponse = await _client.PostAsJsonAsync("/api/workflows", CreateWorkflowRequest(workflowName));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = ResolveWorkflowId(createResponse);

        var dryRunResponse = await _client.PostAsJsonAsync(
            $"/api/workflows/{createdId}/execute",
            new ExecuteWorkflowRequest(DryRun: true, SchemaVersion: 1));

        dryRunResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dryRunPayload = await dryRunResponse.Content.ReadFromJsonAsync<ExecuteWorkflowResponse>();
        dryRunPayload.Should().NotBeNull();
        dryRunPayload!.DryRun.Should().BeTrue();
        dryRunPayload.Persisted.Should().BeFalse();
        dryRunPayload.ExecutionId.Should().BeNull();
        dryRunPayload.WasSuccessful.Should().BeTrue();

        JsonDocument.Parse(dryRunPayload.ResultJson).RootElement.ValueKind.Should().Be(JsonValueKind.Array);

        var persistedResponse = await _client.PostAsJsonAsync(
            $"/api/workflows/{createdId}/execute",
            new ExecuteWorkflowRequest(DryRun: false, SchemaVersion: 1));

        persistedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persistedPayload = await persistedResponse.Content.ReadFromJsonAsync<ExecuteWorkflowResponse>();
        persistedPayload.Should().NotBeNull();
        persistedPayload!.DryRun.Should().BeFalse();
        persistedPayload.Persisted.Should().BeTrue();
        persistedPayload.ExecutionId.Should().NotBeNull();
        persistedPayload.WasSuccessful.Should().BeTrue();
    }

    private static Guid ResolveWorkflowId(HttpResponseMessage response)
    {
        response.Headers.Location.Should().NotBeNull();
        var location = response.Headers.Location!.ToString().TrimEnd('/');
        var workflowIdSegment = location.Split('/').Last();

        return Guid.Parse(workflowIdSegment.TrimEnd('/'));
    }

    private static WorkflowRequest CreateWorkflowRequest(string workflowName) => new(
        Name: workflowName,
        Expression: "1 == 1",
        RuleJson: BuildRuleJson(workflowName),
        Version: 1,
        IsActive: true,
        EffectiveFromUtc: null,
        EffectiveToUtc: null,
        SchemaVersion: 1);

    private static string BuildRuleJson(string workflowName) => $$"""
        {
          "WorkflowName": "{{workflowName}}",
          "Rules": [
            {
              "RuleName": "AlwaysTrue",
              "Expression": "1 == 1"
            }
          ]
        }
        """;
}
