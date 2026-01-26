using ErrorOr;

using Mediator;

using Weda.Core.Application.Security.CurrentUserProvider;
using Weda.Template.Application.Users.Mapping;
using Weda.Template.Contracts.Users.Commands;
using Weda.Template.Contracts.Users.Dtos;
using Weda.Template.Domain.Users.Errors;
using Weda.Template.Domain.Users.Repositories;

namespace Weda.Template.Application.Users.Commands.UpdateUserRoles;

public class UpdateUserRolesCommandHandler(
    IUserRepository userRepository,
    ICurrentUserProvider currentUserProvider) : IRequestHandler<UpdateUserRolesCommand, ErrorOr<UserDto>>
{
    public async ValueTask<ErrorOr<UserDto>> Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound;
        }

        var currentUser = currentUserProvider.GetCurrentUser();

        var updateResult = user.UpdateRoles(request.Roles, currentUser.Id, currentUser.Roles);
        if (updateResult.IsError)
        {
            return updateResult.Errors;
        }

        if (request.Permissions is not null)
        {
            user.UpdatePermissions(request.Permissions);
        }

        await userRepository.UpdateAsync(user, cancellationToken);

        return UserMapper.ToDto(user);
    }
}
