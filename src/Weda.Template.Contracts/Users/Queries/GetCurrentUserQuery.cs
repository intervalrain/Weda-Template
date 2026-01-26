using ErrorOr;
using Mediator;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Queries;

public record GetCurrentUserQuery : IRequest<ErrorOr<UserDto>>;
