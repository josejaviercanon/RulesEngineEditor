using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;
using static System.Net.WebRequestMethods;


var builder = WebApplication.CreateBuilder(args);

// >>===>> Add services to the container.

// Add a CORS policy for the client
// Add .AllowCredentials() for apps that use an Identity Provider for authn/z
builder.Services.AddCors(
    options => options.AddPolicy(
        "localhost",
        policy => policy.WithOrigins("https://localhost")
            .AllowAnyMethod()
            .AllowAnyHeader()));

builder.Services.AddControllers();

builder.Services.AddOpenApi(); // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference();

}

// Activate the CORS policy
app.UseCors("localhost");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/", () => "RulesEngine Editor Web API.");

// >>===>> Add run container.
await app.RunAsync();