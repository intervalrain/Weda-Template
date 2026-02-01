using ErrorOr;

using Mediator;

using Weda.Core.Application.Security.PasswordHasher;
using Weda.Template.Application.Users.Mapping;
using Weda.Template.Contracts.Users.Commands;
using Weda.Template.Contracts.Users.Dtos;
using Weda.Template.Domain.Users.Entities;
using Weda.Template.Domain.Users.Errors;
using Weda.Template.Domain.Users.Repositories;

namespace Weda.Template.Application.Users.Commands.CreateUser;

public class CreateUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher) : IRequestHandler<CreateUserCommand, ErrorOr<UserDto>>
{
    public async ValueTask<ErrorOr<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingUser is not null)
        {
            return UserErrors.DuplicateEmail;
        }

        var passwordHash = passwordHasher.HashPassword(request.Password);

        var userResult = User.Create(
            request.Email,
            passwordHash,
            request.Name);

        if (userResult.IsError)
        {
            return userResult.Errors;
        }

        var user = userResult.Value;
        await userRepository.AddAsync(user, cancellationToken);

        return UserMapper.ToDto(user);
    }
}
