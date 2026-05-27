using Scalar.AspNetCore;

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

app.MapGet("/weatherforecast", () =>
{
    string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        })
        .ToArray();

    return forecast;
})
.WithName("GetWeatherForecast");

await app.RunAsync();

internal sealed record WeatherForecast
{
    public required DateOnly Date { get; init; }

    public required int TemperatureC { get; init; }

    public required string Summary { get; init; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
