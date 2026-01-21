using ErrorOr;

using Mediator;

using Weda.Core.Application.Security;

namespace Weda.Template.Application.Auth.Queries.GetToken;

public class GetTokenQueryHandler(IJwtTokenGenerator jwtTokenGenerator) : IRequestHandler<GetTokenQuery, ErrorOr<string>>
{
    public async ValueTask<ErrorOr<string>> Handle(GetTokenQuery query, CancellationToken cancellationToken)
    {
        var id = query.Id ?? Guid.NewGuid();

        var token = jwtTokenGenerator.GenerateToken(
            id,
            query.Name,
            query.Email,
            query.Permissions,
            query.Roles);

        return token;
    }
}
