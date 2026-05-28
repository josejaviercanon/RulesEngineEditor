using MediatR;
using RulesEngine.Application.Dtos;

namespace RulesEngine.Application.Commands;

public sealed record ValidateWorkflowCommand(
    string RuleJson,
    int? SchemaVersion) : IRequest<WorkflowValidationDto>;
