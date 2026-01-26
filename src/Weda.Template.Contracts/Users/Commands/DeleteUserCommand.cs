using ErrorOr;
using Mediator;

namespace Weda.Template.Contracts.Users.Commands;

public record DeleteUserCommand(Guid Id) : IRequest<ErrorOr<Deleted>>;
