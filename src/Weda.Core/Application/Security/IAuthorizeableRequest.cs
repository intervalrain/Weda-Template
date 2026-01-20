using Mediator;

namespace Weda.Core.Application.Security;

public interface IAuthorizeableRequest<T> : IRequest<T>
{
    Guid UserId { get; }
}
