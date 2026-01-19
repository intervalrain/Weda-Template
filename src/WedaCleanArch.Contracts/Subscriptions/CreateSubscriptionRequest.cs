using WedaCleanArch.Contracts.Common;

namespace WedaCleanArch.Contracts.Subscriptions;

public record CreateSubscriptionRequest(
    string FirstName,
    string LastName,
    string Email,
    SubscriptionType SubscriptionType);