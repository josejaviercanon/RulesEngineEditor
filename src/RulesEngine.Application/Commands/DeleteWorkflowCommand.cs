using MediatR;

namespace RulesEngine.Application.Commands;

public sealed record DeleteWorkflowCommand(Guid Id) : IRequest<bool>;
