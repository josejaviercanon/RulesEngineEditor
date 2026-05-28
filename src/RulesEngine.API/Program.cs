using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using MediatR;
using RulesEngine.API.Contracts;
using RulesEngine.API.Mapping;
using RulesEngine.Application.Commands;
using Scalar.AspNetCore;
using RulesEngine.Application.DependencyInjection;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;
using RulesEngine.Core.Validation;
using RulesEngine.Infrastructure.DependencyInjection;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Infrastructure.Persistence.Entities;

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
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add NSwag services
//builder.Services.AddOpenApiDocument();

// 1. Explicitly name the schema "v1"
builder.Services.AddOpenApi("v1");

var app = builder.Build();

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

workflows.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
    {
        var items = await mediator.Send(new ListWorkflowsQuery(), cancellationToken);
        var response = items.Select(item => item.ToResponse());

        return Results.Ok(response);
    })
    .WithName("ListWorkflows");

workflows.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
    {
        var item = await mediator.Send(new GetWorkflowByIdQuery(id), cancellationToken);

        if (item is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(item.ToResponse());
    })
    .WithName("GetWorkflowById");

workflows.MapPost("/", async (
        WorkflowRequest request,
        IMediator mediator,
        IWorkflowSchemaValidator schemaValidator,
        CancellationToken cancellationToken) =>
    {
        var validationResult = schemaValidator.Validate(request.RuleJson, request.SchemaVersion);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new ValidationErrorResponse(validationResult.ResolvedVersion, validationResult.Errors));
        }

        var workflow = await mediator.Send(new CreateWorkflowCommand(
            request.Name,
            request.Expression,
            request.RuleJson,
            request.Version,
            request.IsActive,
            request.EffectiveFromUtc,
            request.EffectiveToUtc), cancellationToken);

        var response = workflow.ToResponse();

        return Results.Created($"/api/workflows/{workflow.Id}", response);
    })
    .WithName("CreateWorkflow");

workflows.MapPut("/{id:guid}", async (
        Guid id,
        WorkflowRequest request,
        IMediator mediator,
        IWorkflowSchemaValidator schemaValidator,
        CancellationToken cancellationToken) =>
    {
        var validationResult = schemaValidator.Validate(request.RuleJson, request.SchemaVersion);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new ValidationErrorResponse(validationResult.ResolvedVersion, validationResult.Errors));
        }

        var updated = await mediator.Send(new UpdateWorkflowCommand(
            id,
            request.Name,
            request.Expression,
            request.RuleJson,
            request.Version,
            request.IsActive,
            request.EffectiveFromUtc,
            request.EffectiveToUtc), cancellationToken);

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

workflows.MapPost("/validate", async (WorkflowRequest request, IMediator mediator, CancellationToken cancellationToken) =>
    {
        var validationResult = await mediator.Send(new ValidateWorkflowCommand(request.RuleJson, request.SchemaVersion), cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new ValidationErrorResponse(validationResult.ResolvedVersion, validationResult.Errors));
        }

        return Results.Ok(new { validationResult.ResolvedVersion, Errors = Array.Empty<string>() });
    })
    .WithName("ValidateWorkflow");

workflows.MapPost("/{id:guid}/execute", async (
        Guid id,
        ExecuteWorkflowRequest request,
        IMediator mediator,
        CancellationToken cancellationToken) =>
    {
        var result = await mediator.Send(new ExecuteWorkflowCommand(id, request.DryRun, request.SchemaVersion), cancellationToken);

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
            ResultJson: result.ResultJson,
            ExecutionId: result.ExecutionId));
    })
    .WithName("ExecuteWorkflow");

await app.RunAsync();

public partial class Program;
