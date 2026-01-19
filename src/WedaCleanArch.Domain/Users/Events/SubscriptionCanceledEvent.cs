using WedaCleanArch.Domain.Common;

namespace WedaCleanArch.Domain.Users.Events;

public record SubscriptionCanceledEvent(User User, Guid SubscriptionId) : IDomainEvent;