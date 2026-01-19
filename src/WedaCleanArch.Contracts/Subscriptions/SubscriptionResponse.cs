using WedaCleanArch.Contracts.Common;

namespace WedaCleanArch.Contracts.Subscriptions;

public record SubscriptionResponse(
    Guid Id,
    Guid UserId,
    SubscriptionType SubscriptionType);