using AutoMapper;
using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Exceptions;
using RulesEngine.Models;

namespace RulesEngine.Application.Handlers;

public sealed class ValidateWorkflowCommandHandler(IMapper mapper)
    : IRequestHandler<ValidateWorkflowCommand, WorkflowValidationDto>
{
    public Task<WorkflowValidationDto> Handle(ValidateWorkflowCommand request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var workflow = mapper.Map<Workflow>(request.Workflow);

        try
        {
            var engine = new global::RulesEngine.RulesEngine();
            engine.AddOrUpdateWorkflow(workflow);
            return Task.FromResult(new WorkflowValidationDto(true, []));
        }
        catch (RuleValidationException ex)
        {
            var errors = ex.Errors.Select(error => error.ErrorMessage).ToArray();
            return Task.FromResult(new WorkflowValidationDto(false, errors));
        }

    }
}
