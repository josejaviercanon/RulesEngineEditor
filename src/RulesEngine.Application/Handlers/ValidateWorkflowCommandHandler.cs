using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Core.Validation;

namespace RulesEngine.Application.Handlers;

public sealed class ValidateWorkflowCommandHandler(IWorkflowSchemaValidator schemaValidator)
    : IRequestHandler<ValidateWorkflowCommand, WorkflowValidationDto>
{
    public Task<WorkflowValidationDto> Handle(ValidateWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = schemaValidator.Validate(request.RuleJson, request.SchemaVersion);

        return Task.FromResult(new WorkflowValidationDto(result.ResolvedVersion, result.Errors));
    }
}
