using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine.Core.Repositories;
using RulesEngine.Infrastructure.Configuration;
using RulesEngine.Infrastructure.Persistence;
using RulesEngine.Infrastructure.Repositories;

namespace RulesEngine.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Missing database connection string: ConnectionStrings:DefaultConnection");
        }

        services.AddSingleton(new DatabaseConnectionOptions
        {
            DefaultConnection = connectionString
        });

        services.AddDbContext<RulesEngineEditorDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IWorkflowRepository, WorkflowRepository>();
        services.AddScoped<IExecutionStateRepository, ExecutionStateRepository>();

        return services;
    }
}
