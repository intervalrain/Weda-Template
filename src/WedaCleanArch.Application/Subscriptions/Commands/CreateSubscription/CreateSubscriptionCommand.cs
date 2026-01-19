using WedaCleanArch.Application.Common.Security.Permissions;
using WedaCleanArch.Application.Common.Security.Policies;
using WedaCleanArch.Application.Common.Security.Request;
using WedaCleanArch.Application.Subscriptions.Common;
using WedaCleanArch.Domain.Users;

using ErrorOr;

namespace WedaCleanArch.Application.Subscriptions.Commands.CreateSubscription;

[Authorize(Permissions = Permission.Subscription.Create, Policies = Policy.SelfOrAdmin)]
public record CreateSubscriptionCommand(
    Guid UserId,
    string FirstName,
    string LastName,
    string Email,
    SubscriptionType SubscriptionType)
    : IAuthorizeableRequest<ErrorOr<SubscriptionResult>>;