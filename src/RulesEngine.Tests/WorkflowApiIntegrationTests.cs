using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine.API.Contracts;
using RulesEngine.Application.Dtos;
using RulesEngine.Infrastructure.Persistence;
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
        loaded!.Workflow.WorkflowName.Should().Be(workflowName);

        var updateRequest = CreateWorkflowRequest("CrudWorkflowUpdated") with
        {
            Workflow = CreateWorkflowDto("CrudWorkflowUpdated")
        };

        var updateResponse = await _client.PutAsJsonAsync($"/api/workflows/{createdId}", updateRequest);
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var updated = await updateResponse.Content.ReadFromJsonAsync<WorkflowResponse>();
        updated.Should().NotBeNull();
        updated!.Version.Should().Be(2);
        updated.IsActive.Should().BeTrue();

        var versionsResponse = await _client.GetAsync($"/api/workflows/{createdId}/versions");
        versionsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var versions = await versionsResponse.Content.ReadFromJsonAsync<List<WorkflowResponse>>();
        versions.Should().NotBeNull();
        versions!.Should().HaveCount(2);
        versions.Should().ContainSingle(item => item.Version == 1 && !item.IsActive);
        versions.Should().ContainSingle(item => item.Version == 2 && item.IsActive);

        var listResponse = await _client.GetAsync("/api/workflows");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var workflows = await listResponse.Content.ReadFromJsonAsync<List<WorkflowResponse>>();
        workflows.Should().NotBeNull();
        workflows!.Should().Contain(item => item.Id == createdId && item.Workflow.WorkflowName == "CrudWorkflowUpdated");

        var deleteResponse = await _client.DeleteAsync($"/api/workflows/{createdId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getAfterDeleteResponse = await _client.GetAsync($"/api/workflows/{createdId}");
        getAfterDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task WorkflowVersionActivationEndpoint_ShouldSwitchActiveVersionWithoutDeletingHistory()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/workflows", CreateWorkflowRequest("VersionedWorkflow"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = ResolveWorkflowId(createResponse);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/workflows/{createdId}",
            CreateWorkflowRequest("VersionedWorkflowV2"));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var activateResponse = await _client.PostAsync($"/api/workflows/{createdId}/versions/1/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var activated = await activateResponse.Content.ReadFromJsonAsync<WorkflowResponse>();
        activated.Should().NotBeNull();
        activated!.Version.Should().Be(1);
        activated.IsActive.Should().BeTrue();

        var versionsResponse = await _client.GetAsync($"/api/workflows/{createdId}/versions");
        var versions = await versionsResponse.Content.ReadFromJsonAsync<List<WorkflowResponse>>();
        versions.Should().NotBeNull();
        versions!.Should().ContainSingle(item => item.Version == 1 && item.IsActive);
        versions.Should().ContainSingle(item => item.Version == 2 && !item.IsActive);

        var getByIdResponse = await _client.GetAsync($"/api/workflows/{createdId}");
        var current = await getByIdResponse.Content.ReadFromJsonAsync<WorkflowResponse>();
        current.Should().NotBeNull();
        current!.Version.Should().Be(1);
    }

    [Fact]
    public async Task WorkflowVersionReadEndpoint_ShouldReturnRequestedRevision()
    {
        var createResponse = await _client.PostAsJsonAsync("/api/workflows", CreateWorkflowRequest("VersionReadWorkflow"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = ResolveWorkflowId(createResponse);

        var updateResponse = await _client.PutAsJsonAsync(
            $"/api/workflows/{createdId}",
            CreateWorkflowRequest("VersionReadWorkflowV2"));
        updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var versionOneResponse = await _client.GetAsync($"/api/workflows/{createdId}/versions/1");
        versionOneResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var versionOne = await versionOneResponse.Content.ReadFromJsonAsync<WorkflowResponse>();

        versionOne.Should().NotBeNull();
        versionOne!.Version.Should().Be(1);
        versionOne.IsActive.Should().BeFalse();
        versionOne.Workflow.WorkflowName.Should().Be("VersionReadWorkflow");

        var versionTwoResponse = await _client.GetAsync($"/api/workflows/{createdId}/versions/2");
        versionTwoResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var versionTwo = await versionTwoResponse.Content.ReadFromJsonAsync<WorkflowResponse>();

        versionTwo.Should().NotBeNull();
        versionTwo!.Version.Should().Be(2);
        versionTwo.IsActive.Should().BeTrue();
        versionTwo.Workflow.WorkflowName.Should().Be("VersionReadWorkflowV2");

        var missingResponse = await _client.GetAsync($"/api/workflows/{createdId}/versions/99");
        missingResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
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
            new ExecuteWorkflowRequest(DryRun: true, SchemaVersion: 1, Inputs: []));

        dryRunResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var dryRunPayload = await dryRunResponse.Content.ReadFromJsonAsync<ExecuteWorkflowResponse>();
        dryRunPayload.Should().NotBeNull();
        dryRunPayload!.DryRun.Should().BeTrue();
        dryRunPayload.Persisted.Should().BeFalse();
        dryRunPayload.ExecutionId.Should().BeNull();
        dryRunPayload.WasSuccessful.Should().BeTrue();
        dryRunPayload.Results.Should().HaveCount(1);

        var persistedResponse = await _client.PostAsJsonAsync(
            $"/api/workflows/{createdId}/execute",
            new ExecuteWorkflowRequest(DryRun: false, SchemaVersion: 1, Inputs: []));

        persistedResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var persistedPayload = await persistedResponse.Content.ReadFromJsonAsync<ExecuteWorkflowResponse>();
        persistedPayload.Should().NotBeNull();
        persistedPayload!.DryRun.Should().BeFalse();
        persistedPayload.Persisted.Should().BeTrue();
        persistedPayload.ExecutionId.Should().NotBeNull();
        persistedPayload.WasSuccessful.Should().BeTrue();

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<RulesEngineEditorDbContext>();
        var persistedRecord = await dbContext.ExecutionStates
            .AsNoTracking()
            .FirstOrDefaultAsync(state => state.Id == persistedPayload.ExecutionId);

        persistedRecord.Should().NotBeNull();
        persistedRecord!.WorkflowId.Should().Be(createdId);
        persistedRecord.IsDryRun.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateEndpoint_ShouldReturnStructuredErrorsForInvalidWorkflow()
    {
        var invalid = new WorkflowDto();
        var response = await _client.PostAsJsonAsync("/api/workflows/validate", new ValidateWorkflowRequest(invalid));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ValidateWorkflowResponse>();
        payload.Should().NotBeNull();
        payload!.IsValid.Should().BeFalse();
        payload.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ValidateEndpoint_ShouldReturnSuccessForValidWorkflow()
    {
        var valid = CreateWorkflowDto("validate-valid");
        var response = await _client.PostAsJsonAsync("/api/workflows/validate", new ValidateWorkflowRequest(valid));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ValidateWorkflowResponse>();
        payload.Should().NotBeNull();
        payload!.IsValid.Should().BeTrue();
        payload.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ExecuteEndpoint_ShouldReturnValidationErrorForInvalidInputJson()
    {
        const string workflowName = "ExecuteValidationFailure";
        var createResponse = await _client.PostAsJsonAsync("/api/workflows", CreateWorkflowRequest(workflowName));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdId = ResolveWorkflowId(createResponse);

        var response = await _client.PostAsJsonAsync(
            $"/api/workflows/{createdId}/execute",
            new ExecuteWorkflowRequest(
                DryRun: true,
                SchemaVersion: 1,
                Inputs:
                [
                    new RuleParameterDto
                    {
                        Name = "input1",
                        ValueJson = "{not-json}"
                    }
                ]));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var payload = await response.Content.ReadFromJsonAsync<ExecutionErrorResponse>();

        payload.Should().NotBeNull();
        payload!.Code.Should().Be("invalid_input_json");
        payload.Errors.Should().NotBeNull();
        payload.Errors!.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ExecuteEndpoint_ShouldAcceptNamedInputsAndReturnTypedResults()
    {
        var workflow = new WorkflowDto
        {
            WorkflowName = "ExecuteWithInputs",
            Rules =
            [
                new RuleDto
                {
                    RuleName = "InputRule",
                    Enabled = true,
                    Expression = "input1.GetProperty(\"total\").GetInt32() > 10"
                }
            ],
            Version = 1,
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/workflows",
            new WorkflowRequest(workflow, 1));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdId = ResolveWorkflowId(createResponse);
        var executeResponse = await _client.PostAsJsonAsync(
            $"/api/workflows/{createdId}/execute",
            new ExecuteWorkflowRequest(
                DryRun: true,
                SchemaVersion: 1,
                Inputs:
                [
                    new RuleParameterDto
                    {
                        Name = "input1",
                        ValueJson = "{\"total\": 15}"
                    }
                ]));

        executeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await executeResponse.Content.ReadFromJsonAsync<ExecuteWorkflowResponse>();
        payload.Should().NotBeNull();
        payload!.Results.Should().ContainSingle();
        payload.Results[0].RuleName.Should().Be("InputRule");
        payload.Results[0].IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ExecuteEndpoint_ShouldSurfaceExpressionErrorsInResults()
    {
        var workflow = new WorkflowDto
        {
            WorkflowName = "ExecuteExpressionError",
            Rules =
            [
                new RuleDto
                {
                    RuleName = "BrokenRule",
                    Enabled = true,
                    Expression = "input1.GetProperty(\"missing\").GetInt32() > 10"
                }
            ],
            Version = 1,
            IsActive = true
        };

        var createResponse = await _client.PostAsJsonAsync(
            "/api/workflows",
            new WorkflowRequest(workflow, 1));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var createdId = ResolveWorkflowId(createResponse);
        var executeResponse = await _client.PostAsJsonAsync(
            $"/api/workflows/{createdId}/execute",
            new ExecuteWorkflowRequest(
                DryRun: true,
                SchemaVersion: 1,
                Inputs:
                [
                    new RuleParameterDto
                    {
                        Name = "input1",
                        ValueJson = "{\"total\": 15}"
                    }
                ]));

        executeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await executeResponse.Content.ReadFromJsonAsync<ExecuteWorkflowResponse>();
        payload.Should().NotBeNull();
        payload!.WasSuccessful.Should().BeFalse();
        payload.Results.Should().ContainSingle();
        payload.Results[0].ExceptionMessage.Should().NotBeNullOrWhiteSpace();
    }

    private static Guid ResolveWorkflowId(HttpResponseMessage response)
    {
        response.Headers.Location.Should().NotBeNull();
        var location = response.Headers.Location!.ToString().TrimEnd('/');
        var workflowIdSegment = location.Split('/').Last();

        return Guid.Parse(workflowIdSegment.TrimEnd('/'));
    }

    private static WorkflowRequest CreateWorkflowRequest(string workflowName) => new(
        Workflow: CreateWorkflowDto(workflowName),
        SchemaVersion: 1);

    private static WorkflowDto CreateWorkflowDto(string workflowName) => new()
    {
        WorkflowName = workflowName,
        Rules =
        [
            new RuleDto
            {
                RuleName = "AlwaysTrue",
                Enabled = true,
                Expression = "1 == 1"
            }
        ],
        Version = 1,
        IsActive = true
    };
}
