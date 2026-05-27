using System.Text.Json;
using RulesEngine.Core.Validation;

namespace RulesEngine.Application.Validation;

public sealed class JsonWorkflowSchemaValidator : IWorkflowSchemaValidator
{
    private const int DefaultSchemaVersion = 1;

    public SchemaValidationResult Validate(string ruleJson, int? requestedSchemaVersion = null)
    {
        var errors = new List<string>();
        var resolvedVersion = requestedSchemaVersion ?? DefaultSchemaVersion;

        if (resolvedVersion <= 0)
        {
            errors.Add("SchemaVersion must be greater than 0.");
        }

        if (string.IsNullOrWhiteSpace(ruleJson))
        {
            errors.Add("RuleJson is required.");
            return new SchemaValidationResult(resolvedVersion, errors);
        }

        try
        {
            using var document = JsonDocument.Parse(ruleJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                errors.Add("RuleJson must be a JSON object.");
            }
        }
        catch (JsonException)
        {
            errors.Add("RuleJson must be valid JSON.");
        }

        return new SchemaValidationResult(resolvedVersion, errors);
    }
}
