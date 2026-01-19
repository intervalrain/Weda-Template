using WedaCleanArch.Application.Common.Security.Permissions;
using WedaCleanArch.Application.Common.Security.Policies;
using WedaCleanArch.Application.Common.Security.Request;
using WedaCleanArch.Domain.Reminders;

using ErrorOr;

namespace WedaCleanArch.Application.Reminders.Queries.ListReminders;

[Authorize(Permissions = Permission.Reminder.Get, Policies = Policy.SelfOrAdmin)]
public record ListRemindersQuery(Guid UserId, Guid SubscriptionId) : IAuthorizeableRequest<ErrorOr<List<Reminder>>>;