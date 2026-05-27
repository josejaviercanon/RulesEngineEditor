using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RulesEngine.Infrastructure.Configuration;
using RulesEngine.Infrastructure.Persistence;

namespace RulesEngine.Tests.Infrastructure;

public sealed class WorkflowApiFactory : WebApplicationFactory<Program>
{
    private static readonly InMemoryDatabaseRoot DatabaseRoot = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<RulesEngineEditorDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<RulesEngineEditorDbContext>>();

            var connectionOptionsDescriptor = services.SingleOrDefault(
                descriptor => descriptor.ServiceType == typeof(DatabaseConnectionOptions));

            if (connectionOptionsDescriptor is not null)
            {
                services.Remove(connectionOptionsDescriptor);
            }

            services.AddSingleton(new DatabaseConnectionOptions
            {
                DefaultConnection = "InMemory"
            });

            services.AddDbContext<RulesEngineEditorDbContext>(options =>
                options.UseInMemoryDatabase("workflow-api-tests", DatabaseRoot));

            var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<RulesEngineEditorDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }
}
