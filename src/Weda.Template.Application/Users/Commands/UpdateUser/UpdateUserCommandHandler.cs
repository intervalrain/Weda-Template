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

        if (request.Email is not null)
        {
            var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (existingUser is not null && existingUser.Id != user.Id)
            {
                return UserErrors.DuplicateEmail;
            }

            var updateEmailResult = user.UpdateEmail(request.Email);
            if (updateEmailResult.IsError)
            {
                return updateEmailResult.Errors;
            }
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
