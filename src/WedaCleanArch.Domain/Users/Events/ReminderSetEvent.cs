using WedaCleanArch.Domain.Common;
using WedaCleanArch.Domain.Reminders;

namespace WedaCleanArch.Domain.Users.Events;

public record ReminderSetEvent(Reminder Reminder) : IDomainEvent;