using ErrorOr;
using Weda.Core.Application.Interfaces;
using Weda.Template.Contracts.Users.Dtos;

namespace Weda.Template.Contracts.Users.Queries;

public record ListUsersQuery : IQuery<ErrorOr<List<UserDto>>>;
