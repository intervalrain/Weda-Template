using Weda.Core.Application.Security;
using Weda.Core.Application.Security.CurrentUserProvider;

using ErrorOr;
using Weda.Core.Application.Security.Policies;
using Weda.Core.Application.Security.Roles;

namespace Weda.Template.Infrastructure.Security.PolicyEnforcer;

public class PolicyEnforcer : IPolicyEnforcer
{
    public ErrorOr<Success> Authorize<T>(
        IAuthorizeableRequest<T> request,
        CurrentUser currentUser,
        string policy)
    {
        return policy switch
        {
            Policy.SelfOrAdmin => SelfOrAdminPolicy(request, currentUser),
            Policy.SuperAdminOnly => SuperAdminOnlyPolicy(currentUser),
            _ => Error.Unexpected(description: "Unknown policy name"),
        };
    }

    private static ErrorOr<Success> SelfOrAdminPolicy<T>(IAuthorizeableRequest<T> request, CurrentUser currentUser) =>
        request.UserId == currentUser.Id || currentUser.Roles.Contains(Role.Admin) || currentUser.Roles.Contains(Role.SuperAdmin)
            ? Result.Success
            : Error.Unauthorized(description: "Requesting user failed policy requirement");

    private static ErrorOr<Success> SuperAdminOnlyPolicy(CurrentUser currentUser) =>
        currentUser.Roles.Contains(Role.SuperAdmin)
            ? Result.Success
            : Error.Unauthorized(description: "Only SuperAdmin can perform this action");
}