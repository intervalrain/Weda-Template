using Weda.Core.Application.Security.CurrentUserProvider;

namespace Weda.Template.TestCommon.Security;

public class TestCurrentUserProvider : ICurrentUserProvider
{
    private CurrentUser? _currentUser;

    public void Returns(CurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    public CurrentUser GetCurrentUser() => _currentUser ?? CurrentUserFactory.CreateCurrentUser();
}
