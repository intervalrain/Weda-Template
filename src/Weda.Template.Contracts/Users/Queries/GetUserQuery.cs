using ErrorOr;


using Weda.Core.Application.Security;
using Weda.Core.Application.Security.Models;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Queries;

[Authorize(Policies = Policy.SelfOrAdmin)]
public record GetUserQuery(Guid Id) : IAuthorizeableRequest<ErrorOr<UserDto>>
{
    public Guid UserId => Id;
}
