using ErrorOr;
using Weda.Core.Application.Interfaces;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Commands;

public record UpdateUserRolesCommand(
    Guid Id,
    List<string> Roles,
    List<string>? Permissions = null) : ICommand<ErrorOr<UserDto>>;
