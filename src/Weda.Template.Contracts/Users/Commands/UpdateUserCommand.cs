using ErrorOr;
using Mediator;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Commands;

public record UpdateUserCommand(
    Guid Id,
    string? Email = null,
    string? Name = null,
    string? Password = null) : IRequest<ErrorOr<UserDto>>;
