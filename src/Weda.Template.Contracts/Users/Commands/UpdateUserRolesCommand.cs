using ErrorOr;
using Mediator;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Commands;

public record UpdateUserRolesCommand(
    Guid Id,
    List<string> Roles,
    List<string>? Permissions = null) : IRequest<ErrorOr<UserDto>>;
