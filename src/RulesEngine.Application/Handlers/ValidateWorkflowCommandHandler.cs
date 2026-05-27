using MediatR;
using RulesEngine.Application.Commands;

namespace RulesEngine.Application.Handlers;

public sealed class ValidateWorkflowCommandHandler : IRequestHandler<ValidateWorkflowCommand, IReadOnlyCollection<string>>
{
    public Task<IReadOnlyCollection<string>> Handle(ValidateWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(request.RuleJson))
        {
            errors.Add("RuleJson is required.");
        }

        return Task.FromResult<IReadOnlyCollection<string>>(errors);
    }
}
