using RulesEngine.API.Contracts;
using RulesEngine.Application.Dtos;

namespace RulesEngine.API.Mapping;

public static class WorkflowMapping
{
    public static WorkflowResponse ToResponse(this WorkflowDto workflow) =>
        new(
            workflow.Id,
            workflow.Name,
            workflow.Expression,
            workflow.RuleJson,
            workflow.Version,
            workflow.IsActive,
            workflow.EffectiveFromUtc,
            workflow.EffectiveToUtc);
}
