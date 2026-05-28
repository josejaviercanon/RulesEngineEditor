namespace RulesEngine.Application.Dtos;

public sealed record WorkflowValidationDto(int ResolvedVersion, IReadOnlyCollection<string> Errors)
{
    public bool IsValid => Errors.Count == 0;
}
