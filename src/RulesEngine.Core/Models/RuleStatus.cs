namespace RulesEngine.Core.Models;

public enum RuleStatus
{
    Draft,
    Failed,
    Disabled,
    Production
}

public static class RuleStatusParser
{
    public const string Draft = "draft";
    public const string Failed = "failed";
    public const string Disabled = "disabled";
    public const string Production = "production";

    public static RuleStatus ParseOrDefault(string? value, RuleStatus fallback = RuleStatus.Draft)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            Draft => RuleStatus.Draft,
            Failed => RuleStatus.Failed,
            Disabled => RuleStatus.Disabled,
            Production => RuleStatus.Production,
            _ => fallback
        };
    }

    public static bool TryParse(string? value, out RuleStatus status)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            status = default;
            return false;
        }

        switch (value.Trim().ToLowerInvariant())
        {
            case Draft:
                status = RuleStatus.Draft;
                return true;
            case Failed:
                status = RuleStatus.Failed;
                return true;
            case Disabled:
                status = RuleStatus.Disabled;
                return true;
            case Production:
                status = RuleStatus.Production;
                return true;
            default:
                status = default;
                return false;
        }
    }

    public static string ToValue(RuleStatus status)
        => status switch
        {
            RuleStatus.Draft => Draft,
            RuleStatus.Failed => Failed,
            RuleStatus.Disabled => Disabled,
            RuleStatus.Production => Production,
            _ => Draft
        };
}
