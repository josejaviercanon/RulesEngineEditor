using AutoMapper;
using MediatR;
using RulesEngine.Application.Commands;
using RulesEngine.Application.Dtos;
using RulesEngine.Application.Policies;
using RulesEngine.Core.Models;
using RulesEngine.Exceptions;
using RulesEngine.Models;

namespace RulesEngine.Application.Handlers;

public sealed class ValidateWorkflowCommandHandler(
    IMapper mapper,
    IRuleStatusPolicy ruleStatusPolicy)
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
            return Task.FromResult(new WorkflowValidationDto(true, [], []));
        }
        catch (RuleValidationException ex)
        {
            var errors = ex.Errors.Select(error => error.ErrorMessage).ToArray();
            var transitions = request.Workflow.Rules
                .Select(rule =>
                {
                    var before = ruleStatusPolicy.NormalizeOrDefault(rule.Status);
                    var after = ruleStatusPolicy.ResolveCompileFailureStatus(before);

                    return new RuleStatusTransitionDto
                    {
                        RuleGuidId = rule.RuleGuidId,
                        RuleName = rule.RuleName,
                        StatusBefore = RuleStatusParser.ToValue(before),
                        StatusAfter = RuleStatusParser.ToValue(after),
                        TransitionReason = before == after ? null : "compile_failed_auto_transition"
                    };
                })
                .ToArray();

            return Task.FromResult(new WorkflowValidationDto(false, errors, transitions));
        }

    }
}
