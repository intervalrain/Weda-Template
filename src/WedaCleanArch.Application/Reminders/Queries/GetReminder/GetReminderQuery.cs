using WedaCleanArch.Application.Common.Security.Permissions;
using WedaCleanArch.Application.Common.Security.Policies;
using WedaCleanArch.Application.Common.Security.Request;
using WedaCleanArch.Domain.Reminders;

using ErrorOr;

namespace WedaCleanArch.Application.Reminders.Queries.GetReminder;

[Authorize(Permissions = Permission.Reminder.Get, Policies = Policy.SelfOrAdmin)]
public record GetReminderQuery(Guid UserId, Guid SubscriptionId, Guid ReminderId) : IAuthorizeableRequest<ErrorOr<Reminder>>;