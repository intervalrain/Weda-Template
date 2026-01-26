using ErrorOr;

using Mediator;

using Weda.Template.Application.Users.Mapping;
using Weda.Template.Contracts.Users.Dtos;
using Weda.Template.Contracts.Users.Queries;
using Weda.Template.Domain.Users.Repositories;

namespace Weda.Template.Application.Users.Queries.ListUsers;

public class ListUsersQueryHandler(
    IUserRepository userRepository) : IRequestHandler<ListUsersQuery, ErrorOr<List<UserDto>>>
{
    public async ValueTask<ErrorOr<List<UserDto>>> Handle(ListUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(cancellationToken);

        return UserMapper.ToDtoList(users);
    }
}
