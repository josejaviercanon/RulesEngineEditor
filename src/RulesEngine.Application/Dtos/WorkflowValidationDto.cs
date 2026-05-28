namespace RulesEngine.Application.Dtos;

public sealed record WorkflowValidationDto(bool IsValid, IReadOnlyCollection<string> Errors);
