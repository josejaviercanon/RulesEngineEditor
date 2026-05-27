using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using RulesEngine.API.Contracts;
using RulesEngine.API.Mapping;
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

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Seed the database
    //await using var scope = app.Services.CreateAsyncScope();
    //await SeedData.InitializeAsync(scope.ServiceProvider);

    app.MapOpenApi();
    app.MapScalarApiReference(); // Maps the Scalar UI playground to /scalar/v1
}

// Activate the CORS policy
app.UseCors("wasm");

app.UseHttpsRedirection();

app.MapGet("/", () => "RulesEngine Editor Web API!");

var workflows = app.MapGroup("/api/workflows")
    .WithTags("Workflows");

workflows.MapGet("/", async (IWorkflowRepository repository, CancellationToken cancellationToken) =>
    {
        var items = await repository.ListAsync(cancellationToken);
        var response = items.Select(item => new WorkflowResponse(
            item.Id,
            item.Name,
            item.Expression,
            item.RuleJson,
            item.Version,
            item.IsActive,
            item.EffectiveFromUtc,
            item.EffectiveToUtc));

        return Results.Ok(response);
    })
    .WithName("ListWorkflows");

workflows.MapGet("/{id:guid}", async (Guid id, IWorkflowRepository repository, CancellationToken cancellationToken) =>
    {
        var item = await repository.GetByIdAsync(id, cancellationToken);

        if (item is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new WorkflowResponse(
            item.Id,
            item.Name,
            item.Expression,
            item.RuleJson,
            item.Version,
            item.IsActive,
            item.EffectiveFromUtc,
            item.EffectiveToUtc));
    })
    .WithName("GetWorkflowById");

workflows.MapPost("/", async (
        WorkflowRequest request,
        IWorkflowRepository repository,
        IWorkflowSchemaValidator schemaValidator,
        CancellationToken cancellationToken) =>
    {
        var validationResult = schemaValidator.Validate(request.RuleJson, request.SchemaVersion);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new ValidationErrorResponse(validationResult.ResolvedVersion, validationResult.Errors));
        }

        var workflow = await repository.CreateAsync(new WorkflowRecord
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Expression = request.Expression,
            RuleJson = request.RuleJson,
            Version = request.Version,
            IsActive = request.IsActive,
            EffectiveFromUtc = request.EffectiveFromUtc,
            EffectiveToUtc = request.EffectiveToUtc
        }, cancellationToken);

        var response = new WorkflowResponse(
            workflow.Id,
            workflow.Name,
            workflow.Expression,
            workflow.RuleJson,
            workflow.Version,
            workflow.IsActive,
            workflow.EffectiveFromUtc,
            workflow.EffectiveToUtc);

        return Results.Created($"/api/workflows/{workflow.Id}", response);
    })
    .WithName("CreateWorkflow");

workflows.MapPut("/{id:guid}", async (
        Guid id,
        WorkflowRequest request,
        IWorkflowRepository repository,
        IWorkflowSchemaValidator schemaValidator,
        CancellationToken cancellationToken) =>
    {
        var validationResult = schemaValidator.Validate(request.RuleJson, request.SchemaVersion);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new ValidationErrorResponse(validationResult.ResolvedVersion, validationResult.Errors));
        }

        var updated = await repository.UpdateAsync(id, new WorkflowRecord
        {
            Name = request.Name,
            Expression = request.Expression,
            RuleJson = request.RuleJson,
            Version = request.Version,
            IsActive = request.IsActive,
            EffectiveFromUtc = request.EffectiveFromUtc,
            EffectiveToUtc = request.EffectiveToUtc
        }, cancellationToken);

        if (updated is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new WorkflowResponse(
            updated.Id,
            updated.Name,
            updated.Expression,
            updated.RuleJson,
            updated.Version,
            updated.IsActive,
            updated.EffectiveFromUtc,
            updated.EffectiveToUtc));
    })
    .WithName("UpdateWorkflow");

workflows.MapDelete("/{id:guid}", async (Guid id, IWorkflowRepository repository, CancellationToken cancellationToken) =>
    {
        var deleted = await repository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    })
    .WithName("DeleteWorkflow");

workflows.MapPost("/validate", (WorkflowRequest request, IWorkflowSchemaValidator schemaValidator) =>
    {
        var validationResult = schemaValidator.Validate(request.RuleJson, request.SchemaVersion);
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
        RulesEngineEditorDbContext dbContext,
        IWorkflowSchemaValidator schemaValidator,
        IRulesEngineWorkflowService rulesEngineWorkflowService,
        CancellationToken cancellationToken) =>
    {
        var workflow = await dbContext.Workflows
            .AsNoTracking()
            .FirstOrDefaultAsync(current => current.Id == id, cancellationToken);

        if (workflow is null)
        {
            return Results.NotFound();
        }

        var validationResult = schemaValidator.Validate(workflow.Definition.RuleJson, request.SchemaVersion);
        if (!validationResult.IsValid)
        {
            return Results.BadRequest(new ExecutionErrorResponse(
                "validation_failed",
                "Workflow schema validation failed.",
                validationResult.ResolvedVersion,
                validationResult.Errors));
        }

        try
        {
            rulesEngineWorkflowService.AddOrUpdateWorkflow(workflow.Definition.RuleJson);
            var executionResults = await rulesEngineWorkflowService.ExecuteAllRulesAsync(workflow.Name);

            var resultPayload = executionResults.Select(result => new
            {
                RuleName = result.Rule?.RuleName,
                result.IsSuccess,
                result.ExceptionMessage
            });

            var resultJson = JsonSerializer.Serialize(resultPayload);
            var wasSuccessful = executionResults.All(result => result.IsSuccess);

            if (request.DryRun)
            {
                return Results.Ok(new ExecuteWorkflowResponse(
                    DryRun: true,
                    SchemaVersion: validationResult.ResolvedVersion,
                    Persisted: false,
                    WasSuccessful: wasSuccessful,
                    ResultJson: resultJson,
                    ExecutionId: null));
            }

            var executionState = new ExecutionStateRecord
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflow.Id,
                IsDryRun = false,
                WasSuccessful = wasSuccessful,
                ExecutedAtUtc = DateTimeOffset.UtcNow,
                ResultJson = resultJson,
                ErrorJson = null
            };

            dbContext.ExecutionStates.Add(executionState);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Results.Ok(new ExecuteWorkflowResponse(
                DryRun: false,
                SchemaVersion: validationResult.ResolvedVersion,
                Persisted: true,
                WasSuccessful: wasSuccessful,
                ResultJson: resultJson,
                ExecutionId: executionState.Id));
        }
        catch (Exception exception)
        {
            return Results.BadRequest(new ExecutionErrorResponse(
                "execution_failed",
                exception.Message,
                request.SchemaVersion,
                null));
        }
    })
    .WithName("ExecuteWorkflow");

await app.RunAsync();

public partial class Program;
