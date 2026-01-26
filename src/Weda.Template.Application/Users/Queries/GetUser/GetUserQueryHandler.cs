using ErrorOr;

using Mediator;

using Weda.Template.Application.Users.Mapping;
using Weda.Template.Contracts.Users.Dtos;
using Weda.Template.Contracts.Users.Queries;
using Weda.Template.Domain.Users.Errors;
using Weda.Template.Domain.Users.Repositories;

namespace Weda.Template.Application.Users.Queries.GetUser;

public class GetUserQueryHandler(
    IUserRepository userRepository) : IRequestHandler<GetUserQuery, ErrorOr<UserDto>>
{
    public async ValueTask<ErrorOr<UserDto>> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound;
        }

        return UserMapper.ToDto(user);
    }
}
