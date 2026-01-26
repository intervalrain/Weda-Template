using Weda.Core.Application.Security;
using Weda.Core.Application.Security.CurrentUserProvider;

using ErrorOr;

namespace Weda.Template.Infrastructure.Security.PolicyEnforcer;

public interface IPolicyEnforcer
{
    public ErrorOr<Success> Authorize<T>(
        IAuthorizeableRequest<T> request,
        CurrentUser currentUser,
        string policy);
}