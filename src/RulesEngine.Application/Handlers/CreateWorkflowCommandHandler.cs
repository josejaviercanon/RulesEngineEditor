using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Models;
using RulesEngine.Core.Repositories;

namespace RulesEngine.Application.Handlers;

public sealed class CreateWorkflowCommandHandler(IWorkflowRepository workflowRepository)
    : IRequestHandler<CreateWorkflowCommand, WorkflowDto>
{
    public async Task<WorkflowDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var created = await workflowRepository.CreateAsync(new WorkflowRecord
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Expression = request.Expression,
            RuleJson = request.RuleJson,
            Version = request.Version,
            IsActive = request.IsActive,
            EffectiveFromUtc = request.EffectiveFromUtc,
            EffectiveToUtc = request.EffectiveToUtc
        }, cancellationToken);

        return new WorkflowDto
        {
            Id = created.Id,
            Name = created.Name,
            Expression = created.Expression,
            RuleJson = created.RuleJson,
            Version = created.Version,
            IsActive = created.IsActive,
            EffectiveFromUtc = created.EffectiveFromUtc,
            EffectiveToUtc = created.EffectiveToUtc
        };
    }
}
