using ErrorOr;
using Mediator;

namespace Weda.Template.Application.Auth.Queries.GetToken;

public record GetTokenQuery(
    Guid? Id,
    string Name,
    string Email,
    List<string> Permissions,
    List<string> Roles) : IRequest<ErrorOr<string>>;