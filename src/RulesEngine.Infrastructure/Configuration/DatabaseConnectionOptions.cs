namespace RulesEngine.Infrastructure.Configuration;

public sealed class DatabaseConnectionOptions
{
    public const string SectionName = "ConnectionStrings";

    public string DefaultConnection { get; init; } = string.Empty;
}
