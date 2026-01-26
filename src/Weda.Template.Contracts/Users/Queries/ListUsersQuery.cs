using ErrorOr;
using Mediator;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Queries;

public record ListUsersQuery : IRequest<ErrorOr<List<UserDto>>>;
