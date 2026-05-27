using RulesEngine.API.Contracts;
using RulesEngine.Infrastructure.Persistence.Entities;

namespace RulesEngine.API.Mapping;

public static class WorkflowMapping
{
    public static WorkflowResponse ToResponse(this RuleRecord entity) =>
        new(
            entity.Id,
            entity.Name,
            entity.Expression,
            entity.RuleJson,
            entity.Version,
            entity.IsActive,
            entity.EffectiveFromUtc,
            entity.EffectiveToUtc);
}
