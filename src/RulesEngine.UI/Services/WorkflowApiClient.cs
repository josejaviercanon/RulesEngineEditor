using System.Net;
using System.Net.Http.Json;

namespace RulesEngine.UI.Services;

public sealed class WorkflowApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<WorkflowResponsePayload>> ListWorkflowsAsync(CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<WorkflowResponsePayload>>("api/workflows", cancellationToken) ?? [];

    public async Task<WorkflowResponsePayload?> GetWorkflowByIdAsync(
        Guid id,
        string mode = "ActiveOnly",
        CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<WorkflowResponsePayload>(
            $"api/workflows/{id}?mode={Uri.EscapeDataString(mode)}",
            cancellationToken);

    public async Task<IReadOnlyList<WorkflowResponsePayload>> ListWorkflowVersionsAsync(Guid id, CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<List<WorkflowResponsePayload>>($"api/workflows/{id}/versions", cancellationToken) ?? [];

    public async Task<WorkflowResponsePayload> CreateWorkflowAsync(
        WorkflowPayload workflow,
        int? schemaVersion = 1,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/workflows",
            new WorkflowRequestPayload(workflow, schemaVersion, CreateNewVersion: false),
            cancellationToken);

        return await ReadWorkflowResponseAsync(response, cancellationToken);
    }

    public async Task<WorkflowResponsePayload> UpdateWorkflowAsync(
        Guid id,
        WorkflowPayload workflow,
        int? schemaVersion = 1,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"api/workflows/{id}",
            new WorkflowRequestPayload(workflow, schemaVersion, CreateNewVersion: false),
            cancellationToken);

        return await ReadWorkflowResponseAsync(response, cancellationToken);
    }

    public async Task<WorkflowResponsePayload> CreateWorkflowVersionAsync(
        Guid id,
        WorkflowPayload workflow,
        int? schemaVersion = 1,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"api/workflows/{id}",
            new WorkflowRequestPayload(workflow, schemaVersion, CreateNewVersion: true),
            cancellationToken);

        return await ReadWorkflowResponseAsync(response, cancellationToken);
    }

    public async Task<WorkflowResponsePayload> ActivateWorkflowVersionAsync(Guid id, int version, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync($"api/workflows/{id}/versions/{version}/activate", null, cancellationToken);
        return await ReadWorkflowResponseAsync(response, cancellationToken);
    }

    public async Task<RulePayload> ActivateRuleVersionAsync(
        Guid workflowId,
        Guid ruleGuidId,
        int version,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync(
            $"api/workflows/{workflowId}/rules/{ruleGuidId}/versions/{version}/activate",
            null,
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadValidationErrorAsync(response, cancellationToken);
            throw new WorkflowApiException(error.Errors.FirstOrDefault() ?? "Failed to activate rule version.", error.Errors);
        }

        var payload = await response.Content.ReadFromJsonAsync<RulePayload>(cancellationToken);
        return payload ?? throw new WorkflowApiException("Rule activation returned an empty response payload.", []);
    }

    public async Task<WorkflowResponsePayload> SetWorkflowVersionEnabledAsync(
        Guid id,
        int version,
        bool isEnabled,
        CancellationToken cancellationToken = default)
    {
        var action = isEnabled ? "enable" : "disable";
        var response = await httpClient.PostAsync($"api/workflows/{id}/versions/{version}/{action}", null, cancellationToken);
        return await ReadWorkflowResponseAsync(response, cancellationToken);
    }

    public async Task<ValidateWorkflowResponsePayload> ValidateWorkflowAsync(
        WorkflowPayload workflow,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync(
            "api/workflows/validate",
            new ValidateWorkflowRequestPayload(workflow),
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadValidationErrorAsync(response, cancellationToken);
            throw new WorkflowApiException(error.Errors.FirstOrDefault() ?? "Workflow validation failed.", error.Errors);
        }

        return await response.Content.ReadFromJsonAsync<ValidateWorkflowResponsePayload>(cancellationToken)
            ?? new ValidateWorkflowResponsePayload(false, ["Unexpected empty validation response."], []);
    }

    private static async Task<WorkflowResponsePayload> ReadWorkflowResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (!response.IsSuccessStatusCode)
        {
            var error = await ReadValidationErrorAsync(response, cancellationToken);
            throw new WorkflowApiException(error.Errors.FirstOrDefault() ?? "Workflow request failed.", error.Errors);
        }

        var payload = await response.Content.ReadFromJsonAsync<WorkflowResponsePayload>(cancellationToken);

        return payload ?? throw new WorkflowApiException("Workflow request returned an empty response payload.", []);
    }

    private static async Task<ValidationErrorResponsePayload> ReadValidationErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var payload = await response.Content.ReadFromJsonAsync<ValidationErrorResponsePayload>(cancellationToken);

        if (payload is not null)
        {
            return payload;
        }

        var fallback = response.StatusCode switch
        {
            HttpStatusCode.NotFound => "Requested resource was not found.",
            HttpStatusCode.BadRequest => "Request validation failed.",
            _ => $"Unexpected API error ({(int)response.StatusCode})."
        };

        return new ValidationErrorResponsePayload(1, [fallback]);
    }
}

public sealed class WorkflowApiException(string message, IReadOnlyList<string> errors) : Exception(message)
{
    public IReadOnlyList<string> Errors { get; } = errors;
}

public sealed record WorkflowRequestPayload(WorkflowPayload Workflow, int? SchemaVersion, bool CreateNewVersion = false);

public sealed record WorkflowResponsePayload(
    Guid Id,
    WorkflowPayload Workflow,
    int Version,
    int ActiveVersion,
    int LastVersion,
    bool IsActive,
    bool IsEnabled,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc);

public sealed record ValidationErrorResponsePayload(int SchemaVersion, IReadOnlyList<string> Errors);

public sealed record ValidateWorkflowRequestPayload(WorkflowPayload Workflow);

public sealed record ValidateWorkflowResponsePayload(
    bool IsValid,
    IReadOnlyList<string> Errors,
    IReadOnlyList<RuleStatusTransitionPayload> RuleStatusTransitions);

public sealed record RuleStatusTransitionPayload(
    string RuleName,
    string RuleGuidId,
    int Version,
    string PreviousStatus,
    string CurrentStatus,
    string Reason);

public sealed class WorkflowPayload
{
    public Guid Id { get; set; }

    public string WorkflowName { get; set; } = string.Empty;

    public string RuleExpressionType { get; set; } = "LambdaExpression";

    public IReadOnlyList<ScopedParamPayload> GlobalParams { get; set; } = [];

    public IReadOnlyList<RulePayload> Rules { get; set; } = [];

    public IReadOnlyList<string> WorkflowsToInject { get; set; } = [];

    public string WorkflowJson { get; set; } = string.Empty;

    public int Version { get; set; }

    public bool IsActive { get; set; }

    public bool IsEnabled { get; set; } = true;

    public string? Comments { get; set; }

    public DateTimeOffset? EffectiveFromUtc { get; set; }

    public DateTimeOffset? EffectiveToUtc { get; set; }
}

public sealed class RulePayload
{
    public Guid RuleGuidId { get; set; }

    public int Version { get; set; }

    public bool IsActive { get; set; }

    public string Status { get; set; } = "draft";

    public string RuleName { get; set; } = string.Empty;

    public string? Operator { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public string RuleExpressionType { get; set; } = "LambdaExpression";

    public string Expression { get; set; } = string.Empty;

    public string RuleJson { get; set; } = string.Empty;

    public string SuccessEvent { get; set; } = string.Empty;

    public IReadOnlyList<ScopedParamPayload> LocalParams { get; set; } = [];

    public IReadOnlyList<RulePayload> Rules { get; set; } = [];

    public RuleActionsPayload? Actions { get; set; }

    public IReadOnlyList<string> WorkflowsToInject { get; set; } = [];

    public IReadOnlyDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
}

public sealed class ScopedParamPayload
{
    public string Name { get; set; } = string.Empty;

    public string Expression { get; set; } = string.Empty;
}

public sealed class RuleActionsPayload
{
    public ActionInfoPayload? OnSuccess { get; set; }

    public ActionInfoPayload? OnFailure { get; set; }
}

public sealed class ActionInfoPayload
{
    public string Name { get; set; } = string.Empty;

    public IReadOnlyDictionary<string, object?> Context { get; set; } = new Dictionary<string, object?>();
}
