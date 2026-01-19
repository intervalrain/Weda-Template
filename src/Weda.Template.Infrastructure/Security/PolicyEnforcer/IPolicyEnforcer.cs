using Weda.Template.Application.Common.Security.Request;
using Weda.Template.Infrastructure.Security.CurrentUserProvider;

using ErrorOr;

namespace Weda.Template.Infrastructure.Security.PolicyEnforcer;

public interface IPolicyEnforcer
{
    public ErrorOr<Success> Authorize<T>(
        IAuthorizeableRequest<T> request,
        CurrentUser currentUser,
        string policy);
}