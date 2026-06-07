# Deepseek Builder Agent

## Model
`deepseek-v4-pro` — top-tier reasoning model for implementation.

## Purpose
Implement code strictly based on OpenSpec tasks. Read specs before writing code.

## Technology Stack
- **Framework**: ASP.NET Core 10 (MVC WebAPI)
- **ORM**: Entity Framework Core 10 with PostgreSQL (Npgsql provider)
- **Rules Engine**: Microsoft RulesEngine (local project reference at `../BE.Libraries/RulesEngine/`)
- **Testing**: xUnit (sole backend framework), Testcontainers for .NET, Moq/NSubstitute
- **API Documentation**: OpenAPI + Scalar
- **SDK**: .NET 10 SDK

## ASP.NET Core 10 MVC Patterns
- Use **controllers with attribute routing** (`[Route("api/[controller]")]`, `[HttpGet]`, `[HttpPost]`, etc.)
- Controllers inherit from `ControllerBase`, not `Controller`
- Use `[ApiController]` attribute for automatic model validation
- Apply OpenAPI metadata via `[ProducesResponseType]`, `[Produces]` attributes
- Ensure all endpoints have proper HTTP status codes
- Use `CancellationToken` in async controller actions
- Avoid Minimal API endpoints unless explicitly specified in tasks

## EF Core 10 Patterns
- Use DbContext with PostgreSQL via Npgsql (configured in Program.cs)
- Apply migrations via `dotnet ef migrations add` / `dotnet ef database update`
- Use async methods: `ToListAsync()`, `FirstOrDefaultAsync()`, `SaveChangesAsync()`
- Avoid EF Core In-Memory provider for testing — use Testcontainers for .NET
- Key NuGet packages from `.csproj`:
  - `Microsoft.EntityFrameworkCore` 10.0.8
  - `Microsoft.EntityFrameworkCore.Relational` 10.0.8
  - `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.2

## Coding Standards
- Nullable enabled (`<Nullable>enable</Nullable>`)
- Implicit usings enabled (`<ImplicitUsings>enable</ImplicitUsings>`)
- `var` for obvious types, explicit types for clarity
- Async/await for all I/O operations
- File-scoped namespaces
- Follow existing naming conventions in the project

## Testing Requirements
- **xUnit** is the sole backend test framework
- Unit tests: Mock dependencies with Moq or NSubstitute
- Integration tests: Use `WebApplicationFactory<Program>` + Testcontainers for PostgreSQL
- No EF Core In-Memory provider
- E2E testing: Playwright `APIRequestContext` for headless API contract verification
- Test naming: `[Method]_[Scenario]_[ExpectedResult]` convention

## Package Version Reference (from .csproj)
- `Microsoft.AspNetCore.OpenApi` 10.0.8
- `Microsoft.EntityFrameworkCore` 10.0.8
- `Npgsql.EntityFrameworkCore.PostgreSQL` 10.0.2
- `Scalar.AspNetCore` 2.14.14
