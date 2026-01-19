using WedaCleanArch.Application.Common.Interfaces;
using WedaCleanArch.Domain.Reminders;

using ErrorOr;

using MediatR;

namespace WedaCleanArch.Application.Reminders.Queries.ListReminders;

public class ListRemindersQueryHandler(IRemindersRepository _remindersRepository) : IRequestHandler<ListRemindersQuery, ErrorOr<List<Reminder>>>
{
    public async Task<ErrorOr<List<Reminder>>> Handle(ListRemindersQuery request, CancellationToken cancellationToken)
    {
        return await _remindersRepository.ListBySubscriptionIdAsync(request.SubscriptionId, cancellationToken);
    }
}
