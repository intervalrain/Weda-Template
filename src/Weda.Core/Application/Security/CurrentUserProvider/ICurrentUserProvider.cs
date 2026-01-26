namespace Weda.Core.Application.Security.CurrentUserProvider;

public interface ICurrentUserProvider
{
    CurrentUser GetCurrentUser();
}
