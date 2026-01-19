using WedaCleanArch.Domain.Common;

namespace WedaCleanArch.Domain.Users.Events;

public record ReminderDeletedEvent(Guid ReminderId) : IDomainEvent;