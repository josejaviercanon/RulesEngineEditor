using RulesEngine.API.Contracts;
using RulesEngine.Application.Dtos;

namespace RulesEngine.API.Mapping;

public static class WorkflowMapping
{
    public static WorkflowResponse ToResponse(this WorkflowDto workflow) =>
        new(
            workflow.Id,
            workflow,
            workflow.Version,
            workflow.ActiveVersion,
            workflow.LastVersion,
            workflow.IsActive,
            workflow.IsEnabled,
            workflow.EffectiveFromUtc,
            workflow.EffectiveToUtc);
}
