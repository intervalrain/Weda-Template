using WedaCleanArch.Application.Common.Interfaces;
using WedaCleanArch.Application.Subscriptions.Common;
using WedaCleanArch.Domain.Users;

using ErrorOr;

using MediatR;

namespace WedaCleanArch.Application.Subscriptions.Queries.GetSubscription;

public class GetSubscriptionQueryHandler(IUsersRepository _usersRepository)
    : IRequestHandler<GetSubscriptionQuery, ErrorOr<SubscriptionResult>>
{
    public async Task<ErrorOr<SubscriptionResult>> Handle(GetSubscriptionQuery request, CancellationToken cancellationToken)
    {
        return await _usersRepository.GetByIdAsync(request.UserId, cancellationToken) is User user
            ? SubscriptionResult.FromUser(user)
            : Error.NotFound(description: "Subscription not found.");
    }
}
