using WedaCleanArch.Domain.Common;

namespace WedaCleanArch.Domain.Users.Events;

public record ReminderDismissedEvent(Guid ReminderId) : IDomainEvent;