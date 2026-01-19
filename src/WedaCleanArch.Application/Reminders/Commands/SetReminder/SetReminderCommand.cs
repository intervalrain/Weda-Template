using WedaCleanArch.Application.Common.Security.Permissions;
using WedaCleanArch.Application.Common.Security.Policies;
using WedaCleanArch.Application.Common.Security.Request;
using WedaCleanArch.Domain.Reminders;

using ErrorOr;

namespace WedaCleanArch.Application.Reminders.Commands.SetReminder;

[Authorize(Permissions = Permission.Reminder.Set, Policies = Policy.SelfOrAdmin)]
public record SetReminderCommand(Guid UserId, Guid SubscriptionId, string Text, DateTime DateTime)
    : IAuthorizeableRequest<ErrorOr<Reminder>>;