using WedaCleanArch.Application.Common.Security.Request;
using WedaCleanArch.Infrastructure.Security.CurrentUserProvider;

using ErrorOr;

namespace WedaCleanArch.Infrastructure.Security.PolicyEnforcer;

public interface IPolicyEnforcer
{
    public ErrorOr<Success> Authorize<T>(
        IAuthorizeableRequest<T> request,
        CurrentUser currentUser,
        string policy);
}