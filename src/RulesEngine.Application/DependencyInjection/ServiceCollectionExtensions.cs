using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RulesEngine.Application.Validation;
using RulesEngine.Core.Execution;
using RulesEngine.Core.Validation;

namespace RulesEngine.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        _ = configuration;
        services.AddAutoMapper(typeof(ServiceCollectionExtensions).Assembly);
        services.AddMediatR(config =>
            config.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));
        services.AddSingleton<IRulesEngineWorkflowService, RulesEngineWorkflowService>();
        services.AddScoped<IWorkflowSchemaValidator, JsonWorkflowSchemaValidator>();

        return services;
    }
}
