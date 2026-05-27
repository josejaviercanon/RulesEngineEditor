using RulesEngine.Editor.Shared.Models;

namespace RulesEngine.Editor.Shared.Validation;

public interface IWorkflowDefinitionValidator
{
    IReadOnlyCollection<string> Validate(WorkflowDefinitionDto workflow);
}
