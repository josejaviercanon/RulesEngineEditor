using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using MediatR;
using RulesEngine.API.Contracts;
using RulesEngine.API.Mapping;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using Scalar.AspNetCore;
using RulesEngine.Application.DependencyInjection;
using RulesEngine.Infrastructure.DependencyInjection;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Core.Models;

var builder = WebApplication.CreateBuilder(args);
var backendUrl = builder.Configuration["BackendUrl"] ?? "https://localhost:7086";
var frontendUrl = builder.Configuration["FrontendUrl"] ?? "https://localhost:7286";
var frontendHttpUrl = builder.Configuration["FrontendHttpUrl"] ?? "http://localhost:5062";

// Add a CORS policy for the client
// Add .AllowCredentials() for apps that use an Identity Provider for authn/z
builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins([backendUrl, frontendUrl, frontendHttpUrl])
            .AllowAnyMethod()
            .AllowAnyHeader()));

// Add Endpoints API Explorer
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add NSwag services
//builder.Services.AddOpenApiDocument();

// 1. Explicitly name the schema "v1"
builder.Services.AddOpenApi("v1");

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<RulesEngineEditorDbContext>();
    if (dbContext.Database.IsRelational())
    {
        await dbContext.Database.MigrateAsync();
    }
}

// Seed the database
//await using var scope = app.Services.CreateAsyncScope();
//await SeedData.InitializeAsync(scope.ServiceProvider);

//app.MapOpenApi();

// 2. Explicitly map the route pattern
app.MapOpenApi("/openapi/{documentName}.json");

if (app.Environment.IsDevelopment())
{

    //app.MapScalarApiReference();
    app.MapScalarApiReference(options =>
    {
        // Points explicitly to the exact localized JSON path
        options.WithOpenApiRoutePattern("/openapi/v1.json");
    });

    //app.MapGet("/scalar/v1/", () => Results.Redirect("/scalar/", permanent: false));
}

// Activate the CORS policy
app.UseCors("wasm");

app.UseHttpsRedirection();

app.MapGet("/", () => "RulesEngine Editor Web API!");

var workflows = app.MapGroup("/api/workflows")
    .WithTags("Workflows");

workflows.MapGet("/", async (
        IMediator mediator,
    CancellationToken cancellationToken,
    WorkflowRuleQueryMode mode = WorkflowRuleQueryMode.ActiveOnly) =>
    {
        var items = await mediator.Send(new ListWorkflowsQuery(mode), cancellationToken);
        var response = items.Select(item => item.ToResponse());

        return Results.Ok(response);
    })
    .WithName("ListWorkflows");

workflows.MapGet("/{id:guid}", async (
        Guid id,
        IMediator mediator,
    CancellationToken cancellationToken,
    WorkflowRuleQueryMode mode = WorkflowRuleQueryMode.ActiveOnly) =>
    {
        var item = await mediator.Send(new GetWorkflowByIdQuery(id, mode), cancellationToken);

        if (item is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(item.ToResponse());
    })
    .WithName("GetWorkflowById");

workflows.MapGet("/{id:guid}/versions", async (
        Guid id,
        IMediator mediator,
    CancellationToken cancellationToken,
    WorkflowRuleQueryMode mode = WorkflowRuleQueryMode.ActiveOnly) =>
    {
        var items = await mediator.Send(new ListWorkflowVersionsQuery(id, mode), cancellationToken);
        var response = items.Select(item => item.ToResponse());

        return Results.Ok(response);
    })
    .WithName("ListWorkflowVersions");

workflows.MapGet("/{id:guid}/versions/{version:int}", async (
        Guid id,
        int version,
        IMediator mediator,
    CancellationToken cancellationToken,
    WorkflowRuleQueryMode mode = WorkflowRuleQueryMode.ActiveOnly) =>
    {
        var item = await mediator.Send(new GetWorkflowVersionQuery(id, version, mode), cancellationToken);

        if (item is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(item.ToResponse());
    })
    .WithName("GetWorkflowVersion");

workflows.MapGet("/{id:guid}/rules/{ruleGuidId:guid}/versions", async (
        Guid id,
        Guid ruleGuidId,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var items = await mediator.Send(new ListRuleVersionsQuery(id, ruleGuidId), cancellationToken);
        return Results.Ok(items);
    })
    .WithName("ListRuleVersions");

workflows.MapPost("/{id:guid}/rules/{ruleGuidId:guid}/versions/{version:int}/activate", async (
        Guid id,
        Guid ruleGuidId,
        int version,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var activated = await mediator.Send(new ActivateRuleVersionCommand(id, ruleGuidId, version), cancellationToken);
        if (activated is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(activated);
    })
    .WithName("ActivateRuleVersion");

workflows.MapPost("/{id:guid}/versions/{version:int}/activate", async (
        Guid id,
        int version,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var activated = await mediator.Send(new ActivateWorkflowVersionCommand(id, version), cancellationToken);

        if (activated is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(activated.ToResponse());
    })
    .WithName("ActivateWorkflowVersion");

workflows.MapPost("/", async (
        WorkflowRequest request,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        WorkflowDto workflow;
        try
        {
            workflow = await mediator.Send(new CreateWorkflowCommand(
                request.Workflow,
                request.SchemaVersion), cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new ValidationErrorResponse(request.SchemaVersion ?? 1, [exception.Message]));
        }

        var response = workflow.ToResponse();

        return Results.Created($"/api/workflows/{workflow.Id}", response);
    })
    .WithName("CreateWorkflow");

workflows.MapPut("/{id:guid}", async (
        Guid id,
        WorkflowRequest request,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        WorkflowDto? updated;
        try
        {
            updated = await mediator.Send(new UpdateWorkflowCommand(
                id,
                request.Workflow,
                request.SchemaVersion), cancellationToken);
        }
        catch (InvalidOperationException exception)
        {
            return Results.BadRequest(new ValidationErrorResponse(request.SchemaVersion ?? 1, [exception.Message]));
        }

        if (updated is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(updated.ToResponse());
    })
    .WithName("UpdateWorkflow");

workflows.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
    {
        var deleted = await mediator.Send(new DeleteWorkflowCommand(id), cancellationToken);
        if (!deleted)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    })
    .WithName("DeleteWorkflow");

workflows.MapPost("/validate", async (ValidateWorkflowRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
        var validationResult = await mediator.Send(new ValidateWorkflowCommand(request.Workflow), cancellationToken);
        return Results.Ok(new ValidateWorkflowResponse(validationResult.IsValid, validationResult.Errors.ToArray()));
    })
    .WithName("ValidateWorkflow");

workflows.MapPost("/{id:guid}/execute", async (
        Guid id,
        ExecuteWorkflowRequest request,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var result = await mediator.Send(new ExecuteWorkflowCommand(id, request.DryRun, request.SchemaVersion, request.Inputs), cancellationToken);

        if (!result.Found)
        {
            return Results.NotFound();
        }

        if (!result.IsSuccess)
        {
            return Results.BadRequest(new ExecutionErrorResponse(
                result.ErrorCode ?? "execution_failed",
                result.ErrorMessage ?? "Execution failed.",
                result.SchemaVersion,
                result.Errors));
        }

        return Results.Ok(new ExecuteWorkflowResponse(
            DryRun: result.DryRun,
            SchemaVersion: result.SchemaVersion ?? 1,
            Persisted: result.Persisted,
            WasSuccessful: result.WasSuccessful,
            Results: result.Results,
            ExecutionId: result.ExecutionId));
    })
    .WithName("ExecuteWorkflow");

await app.RunAsync();

public partial class Program;
