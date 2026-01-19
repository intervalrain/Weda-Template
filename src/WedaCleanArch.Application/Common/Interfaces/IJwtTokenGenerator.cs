using WedaCleanArch.Domain.Users;

namespace WedaCleanArch.Application.Common.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(
        Guid id,
        string firstName,
        string lastName,
        string email,
        SubscriptionType subscriptionType,
        List<string> permissions,
        List<string> roles);
}