using ErrorOr;

using Mediator;

using Weda.Core.Application.Security.PasswordHasher;
using Weda.Template.Application.Users.Mapping;
using Weda.Template.Contracts.Users.Commands;
using Weda.Template.Contracts.Users.Dtos;
using Weda.Template.Domain.Users.Errors;
using Weda.Template.Domain.Users.Repositories;

namespace Weda.Template.Application.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<UpdateUserCommand, ErrorOr<UserDto>>
{
    public async ValueTask<ErrorOr<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.Id, cancellationToken);
        if (user is null)
        {
            return UserErrors.NotFound;
        }

        if (request.Name is null && request.Password is null)
        {
            return UserMapper.ToDto(user);
        }

        if (request.Name is not null)
        {
            var updateNameResult = user.UpdateName(request.Name);
            if (updateNameResult.IsError)
            {
                return updateNameResult.Errors;
            }
        }

        if (request.Password is not null)
        {
            var newPasswordHash = passwordHasher.HashPassword(request.Password);
            user.UpdatePassword(newPasswordHash);
        }

        await userRepository.UpdateAsync(user, cancellationToken);

        return UserMapper.ToDto(user);
    }
}
