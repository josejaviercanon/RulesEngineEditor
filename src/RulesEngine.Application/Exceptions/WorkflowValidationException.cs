namespace RulesEngine.Application.Exceptions;

public sealed class WorkflowValidationException : Exception
{
    public WorkflowValidationException(IReadOnlyList<string> errors)
        : base(errors.FirstOrDefault() ?? "Workflow validation failed.")
    {
        Errors = errors;
    }

    public IReadOnlyList<string> Errors { get; }
}