namespace RulesEngine.Core.Validation;

public sealed record SchemaValidationResult(int ResolvedVersion, IReadOnlyCollection<string> Errors)
{
    public bool IsValid => Errors.Count == 0;
}

public interface IWorkflowSchemaValidator
{
    SchemaValidationResult Validate(string ruleJson, int? requestedSchemaVersion = null);
}
