using WedaCleanArch.Application.Authentication.Queries.Login;
using WedaCleanArch.Domain.Users;

using ErrorOr;

using MediatR;

namespace WedaCleanArch.Application.Tokens.Queries.Generate;

public record GenerateTokenQuery(
    Guid? Id,
    string FirstName,
    string LastName,
    string Email,
    SubscriptionType SubscriptionType,
    List<string> Permissions,
    List<string> Roles) : IRequest<ErrorOr<GenerateTokenResult>>;