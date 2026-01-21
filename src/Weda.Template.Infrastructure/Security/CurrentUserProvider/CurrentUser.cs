namespace Weda.Template.Infrastructure.Security.CurrentUserProvider;

public record CurrentUser(
    Guid Id,
    string Name,
    string Email,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<string> Roles);