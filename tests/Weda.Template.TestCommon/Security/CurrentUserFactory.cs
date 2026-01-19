using Weda.Template.Infrastructure.Security.CurrentUserProvider;

namespace Weda.Template.TestCommon.Security;

public static class CurrentUserFactory
{
    public static CurrentUser CreateCurrentUser(
        Guid? id = null,
        string firstName = "Test",
        string lastName = "User",
        string email = "test@example.com",
        IReadOnlyList<string>? permissions = null,
        IReadOnlyList<string>? roles = null)
    {
        return new CurrentUser(
            id ?? Guid.NewGuid(),
            firstName,
            lastName,
            email,
            permissions ?? [],
            roles ?? []);
    }
}
