using WedaCleanArch.Application.Common.Security.Permissions;
using WedaCleanArch.Application.Common.Security.Policies;
using WedaCleanArch.Application.Common.Security.Request;
using WedaCleanArch.Application.Subscriptions.Common;

using ErrorOr;

namespace WedaCleanArch.Application.Subscriptions.Queries.GetSubscription;

[Authorize(Permissions = Permission.Subscription.Get, Policies = Policy.SelfOrAdmin)]
public record GetSubscriptionQuery(Guid UserId)
    : IAuthorizeableRequest<ErrorOr<SubscriptionResult>>;