using ErrorOr;

using Mediator;

using Weda.Core.Application.Security;

using Weda.Template.Application.Users.Mapping;
using Weda.Template.Contracts.Users.Dtos;
using Weda.Template.Contracts.Users.Queries;
using Weda.Template.Domain.Users.Errors;
using Weda.Template.Domain.Users.Repositories;

namespace Weda.Template.Application.Users.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    IUserRepository userRepository,
    ICurrentUserProvider currentUserProvider) : IRequestHandler<GetCurrentUserQuery, ErrorOr<UserDto>>
{
    public async ValueTask<ErrorOr<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var currentUser = currentUserProvider.GetCurrentUser();

        var user = await userRepository.GetByIdAsync(currentUser.Id, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound;
        }

        return UserMapper.ToDto(user);
    }
}
