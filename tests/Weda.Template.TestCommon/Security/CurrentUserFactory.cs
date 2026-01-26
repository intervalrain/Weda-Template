using Weda.Core.Application.Security.CurrentUserProvider;

namespace Weda.Template.TestCommon.Security;

public static class CurrentUserFactory
{
    public static CurrentUser CreateCurrentUser(
        Guid? id = null,
        string name = "Test User",
        string email = "test@example.com",
        IReadOnlyList<string>? permissions = null,
        IReadOnlyList<string>? roles = null)
    {
        return new CurrentUser(
            id ?? Guid.NewGuid(),
            name,
            email,
            permissions ?? [],
            roles ?? []);
    }
}
