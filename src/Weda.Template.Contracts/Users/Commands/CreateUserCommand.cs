using ErrorOr;
using Mediator;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Commands;

public record CreateUserCommand(
    string Email,
    string Password,
    string Name) : IRequest<ErrorOr<UserDto>>;
