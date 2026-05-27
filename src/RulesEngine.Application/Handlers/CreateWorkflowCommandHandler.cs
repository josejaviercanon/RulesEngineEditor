using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Handlers;

public sealed class CreateWorkflowCommandHandler : IRequestHandler<CreateWorkflowCommand, WorkflowDto>
{
    public Task<WorkflowDto> Handle(CreateWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Shell handler: infrastructure-backed persistence wiring is implemented in a later task.
        var result = new WorkflowDto
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            RuleJson = request.RuleJson,
            Version = request.Version,
            IsActive = request.IsActive
        };

        return Task.FromResult(result);
    }
}
