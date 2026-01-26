using ErrorOr;
using Mediator;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Commands;

public record CreateUserCommand(
    string Email,
    string Password,
    string Name,
    List<string>? Roles = null,
    List<string>? Permissions = null) : IRequest<ErrorOr<UserDto>>;
