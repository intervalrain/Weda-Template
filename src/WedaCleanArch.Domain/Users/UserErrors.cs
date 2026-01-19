using ErrorOr;

namespace WedaCleanArch.Domain.Users;

public static class UserErrors
{
    public static Error CannotCreateMoreRemindersThanSubscriptionAllows { get; } = Error.Validation(
        code: "UserErrors.CannotCreateMoreRemindersThanSubscriptionAllows",
        description: "Cannot create more reminders than subscription allows");
}