using ErrorOr;
using Weda.Core.Application.Security;
using Weda.Core.Application.Security.Models;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Commands;

[Authorize(Policies = Policy.SelfOrAdmin)]
public record UpdateUserCommand(
    Guid Id,
    string? Name = null,
    string? Password = null) : IAuthorizeableCommand<ErrorOr<UserDto>>
{
    public Guid UserId => Id;
}
