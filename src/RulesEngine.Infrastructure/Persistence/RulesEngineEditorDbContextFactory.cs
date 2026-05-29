using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RulesEngine.Infrastructure.Persistence;

public sealed class RulesEngineEditorDbContextFactory : IDesignTimeDbContextFactory<RulesEngineEditorDbContext>
{
    public RulesEngineEditorDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RulesEngineEditorDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("RULES_ENGINE_EDITOR_CONNECTION")
            ?? "Host=localhost;Database=rulesengineeditor;Username=postgres;Password=postgres";

        optionsBuilder.UseNpgsql(connectionString);

        return new RulesEngineEditorDbContext(optionsBuilder.Options);
    }
}