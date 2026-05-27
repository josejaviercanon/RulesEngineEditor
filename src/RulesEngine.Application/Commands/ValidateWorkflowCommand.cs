using MediatR;

namespace RulesEngine.Application.Commands;

public sealed record ValidateWorkflowCommand(
    string RuleJson,
    int? SchemaVersion) : IRequest<IReadOnlyCollection<string>>;
