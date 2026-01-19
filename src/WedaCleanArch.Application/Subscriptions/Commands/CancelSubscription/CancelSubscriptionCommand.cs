using WedaCleanArch.Application.Common.Security.Request;
using WedaCleanArch.Application.Common.Security.Roles;

using ErrorOr;

namespace WedaCleanArch.Application.Subscriptions.Commands.CancelSubscription;

[Authorize(Roles = Role.Admin)]
public record CancelSubscriptionCommand(Guid UserId, Guid SubscriptionId) : IAuthorizeableRequest<ErrorOr<Success>>;