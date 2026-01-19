using WedaCleanArch.Application.Common.Interfaces;
using WedaCleanArch.Domain.Users.Events;

using MediatR;

namespace WedaCleanArch.Application.Reminders.Events;

public class ReminderDismissedEventHandler(IRemindersRepository _remindersRepository) : INotificationHandler<ReminderDismissedEvent>
{
    public async Task Handle(ReminderDismissedEvent notification, CancellationToken cancellationToken)
    {
        var reminder = await _remindersRepository.GetByIdAsync(notification.ReminderId, cancellationToken)
            ?? throw new InvalidOperationException();

        reminder.Dismiss();

        await _remindersRepository.UpdateAsync(reminder, cancellationToken);
    }
}
