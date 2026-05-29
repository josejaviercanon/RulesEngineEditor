using System.Net;
using System.Net.Http.Json;
using RulesEngine.UI.Services;

namespace RulesEngine.Tests.Ui;

internal sealed class UiComponentTestHttpHandler : HttpMessageHandler
{
    public static readonly Guid WorkflowId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid RuleGuidId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    public int ListWorkflowsCalls { get; private set; }
    public int ListVersionsCalls { get; private set; }
    public int GetWorkflowCalls { get; private set; }
    public int CreateWorkflowCalls { get; private set; }
    public int UpdateWorkflowCalls { get; private set; }
    public int ValidateWorkflowCalls { get; private set; }
    public int ActivateRuleVersionCalls { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var path = request.RequestUri?.AbsolutePath ?? string.Empty;
        var query = request.RequestUri?.Query ?? string.Empty;

        if (request.Method == HttpMethod.Get && path == "/api/workflows")
        {
            ListWorkflowsCalls++;
            return Task.FromResult(Json(HttpStatusCode.OK, new[] { BuildWorkflowResponsePayload("Sample Workflow", 1, true, true) }));
        }

        if (request.Method == HttpMethod.Get && path == $"/api/workflows/{WorkflowId}/versions")
        {
            ListVersionsCalls++;
            return Task.FromResult(Json(HttpStatusCode.OK, new[]
            {
                BuildWorkflowResponsePayload("Sample Workflow V1", 1, true, true),
                BuildWorkflowResponsePayload("Sample Workflow V2", 2, false, false)
            }));
        }

        if (request.Method == HttpMethod.Get && path == $"/api/workflows/{WorkflowId}")
        {
            GetWorkflowCalls++;

            var workflow = BuildWorkflowPayload("Sample Workflow", 1, true, true);
            workflow.Rules =
            [
                new RulePayload
                {
                    RuleGuidId = RuleGuidId,
                    Version = 1,
                    IsActive = true,
                    Status = "draft",
                    RuleName = "Sample Rule",
                    Enabled = true,
                    Expression = "1 == 1"
                }
            ];

            return Task.FromResult(Json(HttpStatusCode.OK, new WorkflowResponsePayload(
                WorkflowId,
                workflow,
                1,
                true,
                true,
                null,
                null)));
        }

        if (request.Method == HttpMethod.Post && path == "/api/workflows")
        {
            CreateWorkflowCalls++;
            return Task.FromResult(Json(HttpStatusCode.Created, BuildWorkflowResponsePayload("Created Workflow", 1, true, true)));
        }

        if (request.Method == HttpMethod.Put && path == $"/api/workflows/{WorkflowId}")
        {
            UpdateWorkflowCalls++;
            return Task.FromResult(Json(HttpStatusCode.OK, BuildWorkflowResponsePayload("Updated Workflow", 2, true, true)));
        }

        if (request.Method == HttpMethod.Post && path == "/api/workflows/validate")
        {
            ValidateWorkflowCalls++;
            return Task.FromResult(Json(HttpStatusCode.OK, new ValidateWorkflowResponsePayload(true, [], [])));
        }

        if (request.Method == HttpMethod.Post && path == $"/api/workflows/{WorkflowId}/rules/{RuleGuidId}/versions/1/activate")
        {
            ActivateRuleVersionCalls++;
            return Task.FromResult(Json(HttpStatusCode.OK, new RulePayload
            {
                RuleGuidId = RuleGuidId,
                Version = 1,
                IsActive = true,
                RuleName = "Sample Rule",
                Enabled = true,
                Expression = "1 == 1"
            }));
        }

        if (request.Method == HttpMethod.Post && path.StartsWith($"/api/workflows/{WorkflowId}/versions/", StringComparison.Ordinal))
        {
            return Task.FromResult(Json(HttpStatusCode.OK, BuildWorkflowResponsePayload("Sample Workflow", 1, true, true)));
        }

        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = JsonContent.Create(new ValidationErrorResponsePayload(1, ["Endpoint not mocked in test handler."]))
        });
    }

    private static HttpResponseMessage Json<T>(HttpStatusCode statusCode, T payload)
        => new(statusCode)
        {
            Content = JsonContent.Create(payload)
        };

    private static WorkflowResponsePayload BuildWorkflowResponsePayload(string workflowName, int version, bool isActive, bool isEnabled)
        => new(
            WorkflowId,
            BuildWorkflowPayload(workflowName, version, isActive, isEnabled),
            version,
            isActive,
            isEnabled,
            null,
            null);

    private static WorkflowPayload BuildWorkflowPayload(string workflowName, int version, bool isActive, bool isEnabled)
        => new()
        {
            Id = WorkflowId,
            WorkflowName = workflowName,
            Version = version,
            IsActive = isActive,
            IsEnabled = isEnabled,
            Rules =
            [
                new RulePayload
                {
                    RuleGuidId = RuleGuidId,
                    Version = 1,
                    IsActive = true,
                    Status = "draft",
                    RuleName = "Sample Rule",
                    Enabled = true,
                    Expression = "1 == 1"
                }
            ]
        };
}
