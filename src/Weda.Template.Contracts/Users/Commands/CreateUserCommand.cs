using ErrorOr;
using Weda.Core.Application.Interfaces;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Commands;

public record CreateUserCommand(
    string Email,
    string Password,
    string Name) : ICommand<ErrorOr<UserDto>>;
